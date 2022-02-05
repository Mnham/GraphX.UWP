using GraphX.Common.Exceptions;
using GraphX.Measure;

using QuikGraph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    /// <typeparam name="TVertex">The type of the vertices.</typeparam>
    /// <typeparam name="TEdge">The type of the edges.</typeparam>
    /// <typeparam name="TGraph">The type of the graph.</typeparam>
    public partial class CompoundFDPLayoutAlgorithm<TVertex, TEdge, TGraph> :
        DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, CompoundFDPLayoutParameters>,
        ICompoundLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        /*[ContractInvariantMethod]
        private void InvariantContracts()
        {
            Contract.Invariant(1 <= _phase && _phase <= 3);
            Contract.Invariant(_treeGrowingStep > 0);
        }*/

        private double _temperature = 0;

        //private double _temperatureDelta; //need to be initialized
        private const double TEMPERATURE_LAMBDA = 0.99;

        /// <summary>
        /// <para>Phase of the layout process.</para>
        /// <para>Values: 1,2,3</para>
        /// </summary>
        private int _phase = 1;

        /// <summary>
        /// The steps in the actual phase.
        /// </summary>
        private int _step;

        /// <summary>
        /// The maximal iteration count in the phases.
        /// </summary>
        private /*readonly*/ int[] _maxIterationCounts = new int[3] { 30, 70, 50 };

        /// <summary>
        /// The error thresholds for the phases (calculated inside the Init method).
        /// </summary>
        private readonly double[] _errorThresholds = new double[3];

        /// <summary>
        /// Indicates whether the removed tree-node
        /// has been grown back or not.
        /// </summary>
        private bool _allTreesGrown => _removedRootTreeNodeLevels.Count == 0;

        /// <summary>
        /// Grows back a tree-node level in every 'treeGrowingStep'th step.
        /// </summary>
        private readonly int _treeGrowingStep = 5;

        /// <summary>
        /// The magnitude of the gravity force calculated in the init phased.
        /// </summary>
        private double _gravityForceMagnitude;

        /// <summary>
        /// Has been the gravity center initiated or not.
        /// </summary>
        private bool _gravityCenterCalculated;

        private double _phaseDependentRepulsionMultiplier = 1.0;

        /// <summary>
        /// This method is the skeleton of the layout algorithm.
        /// </summary>
        public override void Compute(CancellationToken cancellationToken)
        {
            //call initialize
            Init(_vertexSizes, _vertexBorders, _layoutTypes);

            //Phases:
            //1: layout the skeleton graph without app. specific and gravitation forces.
            //2: add the removed tree nodes and apply app. specific and gravitation forces.
            //3: stabilization

            /* ********* FOR OPTIMIZATION PURPOSES ********** */
            _maxIterationCounts = new int[3];
            _maxIterationCounts[0] = Parameters.Phase1Iterations;
            _maxIterationCounts[1] = Parameters.Phase2Iterations;
            _maxIterationCounts[2] = Parameters.Phase3Iterations;

            double[] temperatureMultipliers = new double[3]
                                              {
                                                  1.0,
                                                  Parameters.Phase2TemperatureInitialMultiplier,
                                                  Parameters.Phase3TemperatureInitialMultiplier
                                              };

            double initialTemperature = Math.Sqrt(_compoundGraph.VertexCount) * Parameters.IdealEdgeLength;
            double minimalTemperature = initialTemperature * 0.1;
            _temperature = initialTemperature;

            _gravityCenterCalculated = false;

            Random rnd = new Random(Parameters.Seed);
            for (_phase = 1; _phase <= 3; _phase++)
            {
                _temperature = initialTemperature * temperatureMultipliers[_phase - 1];
                _phaseDependentRepulsionMultiplier = _phase < 2 ? 0.5 : 1.0;
                //TODO put back the error and its threshold
                /*double error = _errorThresholds[_phase] + 1;*/
                for (_step = _maxIterationCounts[_phase - 1];
                     (_step > 0 && true/*error > _errorThresholds[_phase - 1] */) || (_phase == 2 && !_allTreesGrown);
                     _step--)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    /*error = 0;*/

                    ApplySpringForces(rnd);
                    ApplyRepulsionForces(cancellationToken, rnd);

                    if (_phase > 1)
                    {
                        ApplyGravitationForces(cancellationToken);
                        ApplyApplicationSpecificForces();
                    }

                    // if (ReportOnIterationEndNeeded)
                    //      SavePositions();

                    CalcNodePositionsAndSizes(cancellationToken);

                    if (_phase == 2 && !_allTreesGrown && _step % _treeGrowingStep == 0)
                    {
                        GrowTreesOneLevel();
                    }

                    _temperature *= TEMPERATURE_LAMBDA;
                    _temperature = Math.Max(_temperature, minimalTemperature);
                }
                if (!_gravityCenterCalculated)
                {
                    _rootCompoundVertex.RecalculateBounds();
                    _gravityCenterCalculated = true;
                }
                //if (_phase == 1)
                _temperature *= Parameters.TemperatureDecreasing;
            }
            SavePositions();
        }

        public override void ResetGraph(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges)
        {
            if (VisitedGraph == null && !TryCreateNewGraph())
            {
                throw new GX_GeneralException("Can't create new graph through reflection. Make sure it support default constructor.");
            }

            VisitedGraph.Clear();
            VisitedGraph.AddVertexRange(vertices);
            VisitedGraph.AddEdgeRange(edges);
        }

        private void SavePositions()
        {
            foreach (KeyValuePair<TVertex, VertexData> vertex in _vertexDatas)
            {
                VertexData v = _vertexDatas[vertex.Key];
                VertexPositions[vertex.Key] = v.Position;
            }

            /*var iterationEndedArgs =
                new CompoundLayoutIterationEventArgs<TVertex, TEdge>(
                    0, 0, string.Empty,
                    VertexPositions,
                    InnerCanvasSizes);*/

            //build the test vertex infos
            Dictionary<TVertex, TestingCompoundVertexInfo> vertexInfos = _vertexDatas.ToDictionary(
                kvp => kvp.Key,
                kvp => new TestingCompoundVertexInfo(
                    kvp.Value.SpringForce,
                    kvp.Value.RepulsionForce,
                    kvp.Value.GravitationForce,
                    kvp.Value.ApplicationForce));

            TestingCompoundLayoutIterationEventArgs<TVertex, TEdge, TestingCompoundVertexInfo, object> iterationEndedArgs =
                new TestingCompoundLayoutIterationEventArgs<TVertex, TEdge, TestingCompoundVertexInfo, object>(
                    0, 0, string.Format("Phase: {0}, Steps: {1}", _phase, _step),
                    VertexPositions,
                    InnerCanvasSizes,
                    vertexInfos,
                    _rootCompoundVertex.InnerCanvasCenter);

            //raise the event
            //OnIterationEnded(iterationEndedArgs);
        }

        private void GrowTreesOneLevel()
        {
            if (_removedRootTreeNodeLevels.Count <= 0)
            {
                return;
            }

            IList<RemovedTreeNodeData> treeNodeDatas = _removedRootTreeNodeLevels.Pop();
            foreach (RemovedTreeNodeData tnd in treeNodeDatas)
            {
                _removedRootTreeNodes.Remove(tnd.Vertex);
                _removedRootTreeEdges.Remove(tnd.Edge);
                _levels[0].Add(tnd.Vertex);
                _compoundGraph.AddVertex(tnd.Vertex);
                _compoundGraph.AddEdge(tnd.Edge);

                TVertex otherVertex = tnd.Edge.GetOtherVertex(tnd.Vertex);
                _vertexDatas[tnd.Vertex].Position = _vertexDatas[otherVertex].Position;
            }
        }

        private Vector GetSpringForce(double idealLength, Point uPos, Point vPos, Size uSize, Size vSize, Random rnd)
        {
            Vector positionVector = (uPos - vPos);
            if (positionVector.Length == 0)
            {
                Vector compensationVector = new Vector(rnd.NextDouble(), rnd.NextDouble());
                positionVector = compensationVector * 2;
                uPos += compensationVector;
                vPos -= compensationVector;
            }
            positionVector.Normalize();

            //get the clipping points
            Point c_u = LayoutUtil.GetClippingPoint(uSize, uPos, vPos);
            Point c_v = LayoutUtil.GetClippingPoint(vSize, vPos, uPos);

            Vector F = c_u - c_v;
            bool isSameDirection = LayoutUtil.IsSameDirection(positionVector, F);
            double length;
            if (isSameDirection)
            {
                length = F.Length - idealLength;
            }
            else
            {
                length = F.Length + idealLength;
            }

            if (F.Length == 0)
            {
                F = -positionVector;
            }

            F.Normalize();
            if (length > 0)
            {
                F *= -1;
            }

            Vector Fs = Math.Pow(length / idealLength, 2) / Parameters.ElasticConstant * F;
            return Fs;
        }

        private Vector GetRepulsionForce(Point uPos, Point vPos, Size uSize, Size vSize, double repulsionRange, Random rnd)
        {
            Vector positionVector = (uPos - vPos);
            if (positionVector.Length == 0)
            {
                Vector compensationVector = new Vector(rnd.NextDouble(), rnd.NextDouble());
                positionVector = compensationVector * 2;
                uPos += compensationVector;
                vPos -= compensationVector;
            }
            positionVector.Normalize();

            Point c_u = LayoutUtil.GetClippingPoint(uSize, uPos, vPos);
            Point c_v = LayoutUtil.GetClippingPoint(vSize, vPos, uPos);

            Vector F = c_u - c_v;
            bool isSameDirection = LayoutUtil.IsSameDirection(positionVector, F);
            Vector Fr = new Vector();

            if (isSameDirection && F.Length > repulsionRange)
            {
                return new Vector();
            }

            double length = Math.Max(1, F.Length);
            //double length = F.LengthSquared;
            length = Math.Pow(isSameDirection ? length / (Parameters.IdealEdgeLength * 2.0) : 1 / length, 2);
            Fr = Parameters.RepulsionConstant / length * positionVector * _phaseDependentRepulsionMultiplier;
            return Fr;
        }

        /// <summary>
        /// Applies the attraction forces (between the end nodes
        /// of the edges).
        /// </summary>
        private void ApplySpringForces(Random rnd)
        {
            foreach (TEdge edge in VisitedGraph.Edges)
            {
                if (!_allTreesGrown && (_removedRootTreeNodes.Contains(edge.Source) || _removedRootTreeNodes.Contains(edge.Target)))
                {
                    continue;
                }
                //get the ideal edge length
                double idealLength = Parameters.IdealEdgeLength;
                VertexData u = _vertexDatas[edge.Source];
                VertexData v = _vertexDatas[edge.Target];
                double multiplier = (u.Level + v.Level) / 2.0 + 1;
                if (IsInterGraphEdge(edge))
                {
                    //idealLength *= (u.Level + v.Level + 1) * Parameters.NestingFactor;
                    idealLength *= 1 + (u.Level + v.Level + 1) * Parameters.NestingFactor;
                    //multiplier = 1;
                }

                Vector Fs = GetSpringForce(idealLength, u.Position, v.Position, u.Size, v.Size, rnd) * multiplier;

                //aggregate the forces
                if ((u.IsFixedToParent && u.MovableParent == null) ^ (v.IsFixedToParent && v.MovableParent == null))
                {
                    Fs *= 2;
                }

                if (!u.IsFixedToParent)
                {
                    u.SpringForce += Fs /* * u.Mass / (u.Mass + v.Mass)*/;
                }
                else if (u.MovableParent != null)
                {
                    u.MovableParent.SpringForce += Fs;
                }
                if (!v.IsFixedToParent)
                {
                    v.SpringForce -= Fs /* * v.Mass / (u.Mass + v.Mass)*/;
                }
                else if (v.MovableParent != null)
                {
                    v.MovableParent.SpringForce -= Fs;
                }
            }
        }

        /// <summary>
        /// Applies the repulsion forces between every node-pair.
        /// </summary>
        private void ApplyRepulsionForces(CancellationToken cancellationToken, Random rnd)
        {
            double repulsionRange = Parameters.IdealEdgeLength * Parameters.SeparationMultiplier;
            for (int i = _levels.Count - 1; i >= 0; i--)
            {
                HashSet<TVertex> checkedVertices = new HashSet<TVertex>();
                foreach (TVertex uVertex in _levels[i])
                {
                    checkedVertices.Add(uVertex);
                    VertexData u = _vertexDatas[uVertex];
                    foreach (TVertex vVertex in _levels[i])
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (checkedVertices.Contains(vVertex))
                        {
                            continue;
                        }

                        VertexData v = _vertexDatas[vVertex];

                        if (u.Parent != v.Parent)
                        {
                            continue; //the two vertex not in the same graph
                        }

                        Vector Fr = GetRepulsionForce(u.Position, v.Position, u.Size, v.Size, repulsionRange, rnd) * Math.Pow(u.Level + 1, 2);

                        if (u.IsFixedToParent ^ v.IsFixedToParent)
                        {
                            Fr *= 2;
                        }

                        if (!u.IsFixedToParent)
                        {
                            u.RepulsionForce += Fr /** u.Mass / (u.Mass + v.Mass)*/;
                        }

                        if (!v.IsFixedToParent)
                        {
                            v.RepulsionForce -= Fr /** v.Mass / (u.Mass + v.Mass)*/;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies the gravitation forces.
        /// </summary>
        private void ApplyGravitationForces(CancellationToken cancellationToken)
        {
            for (int i = _levels.Count - 1; i >= 0; i--)
            {
                foreach (TVertex uVertex in _levels[i])
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    VertexData u = _vertexDatas[uVertex];
                    Point center = u.Parent.InnerCanvasCenter;

                    Vector Fg = center - u.Position;
                    if (Fg.Length == 0)
                    {
                        continue;
                    }

                    double length = Math.Max(1, Fg.Length / (Parameters.IdealEdgeLength * 2.0));
                    Fg.Normalize();
                    Fg *= Parameters.GravitationFactor * _gravityForceMagnitude * Math.Pow(u.Level + 1, 2) / Math.Pow(length, 0.25);
                    u.GravitationForce += Fg;
                }
            }
        }

        /// <summary>
        /// Applies the application specific forces to the vertices.
        /// </summary>
        protected virtual void ApplyApplicationSpecificForces()
        {
        }

        private void CalcNodePositionsAndSizes(CancellationToken cancellationToken)
        {
            for (int i = _levels.Count - 1; i >= 0; i--)
            {
                foreach (TVertex uVertex in _levels[i])
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    VertexData u = _vertexDatas[uVertex];
                    Vector force = u.ApplyForce(_temperature * Math.Max(1, _step) / 100.0 * Parameters.DisplacementLimitMultiplier);
                }
            }
        }

        private bool IsInterGraphEdge(TEdge e)
        {
            return _vertexDatas[e.Source].Parent != _vertexDatas[e.Target].Parent;
        }
    }
}