using GraphX.Common;
using GraphX.Common.Exceptions;
using GraphX.Measure;

using QuikGraph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class ISOMLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, ISOMLayoutParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        #region Private fields

        private Queue<TVertex> _queue;
        private Dictionary<TVertex, ISOMData> _isomDataDict;
        private Point _tempPos;
        private double _adaptation;
        private int _radius;

        #endregion Private fields

        #region Constructors

        public ISOMLayoutAlgorithm(TGraph visitedGraph, ISOMLayoutParameters oldParameters)
            : base(visitedGraph)
        {
            Init(oldParameters);
        }

        public ISOMLayoutAlgorithm(TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions,
                                    ISOMLayoutParameters oldParameters)
            : base(visitedGraph, vertexPositions, oldParameters)
        {
            Init(oldParameters);
        }

        protected void Init(ISOMLayoutParameters oldParameters)
        {
            //init _parameters
            InitParameters(oldParameters);

            _queue = new Queue<TVertex>();
            _isomDataDict = new Dictionary<TVertex, ISOMData>();
            _adaptation = Parameters.InitialAdaption;
        }

        #endregion Constructors

        public override void Compute(CancellationToken cancellationToken)
        {
            if (VisitedGraph.VertexCount == 1)
            {
                if (!VertexPositions.ContainsKey(VisitedGraph.Vertices.First()))
                {
                    VertexPositions.Add(VisitedGraph.Vertices.First(), new Point(0, 0));
                }

                return;
            }

            //initialize vertex positions
            InitializeWithRandomPositions(Parameters.Width, Parameters.Height);

            //initialize ISOM data
            foreach (TVertex vertex in VisitedGraph.Vertices)
            {
                if (!_isomDataDict.TryGetValue(vertex, out ISOMData isomData))
                {
                    isomData = new ISOMData();
                    _isomDataDict[vertex] = isomData;
                }
            }

            _radius = Parameters.InitialRadius;
            Random rnd = new Random(Parameters.Seed);
            for (int epoch = 0; epoch < Parameters.MaxEpoch; epoch++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Adjust(cancellationToken, rnd);

                //Update Parameters
                double factor = Math.Exp(-1 * Parameters.CoolingFactor * (1.0 * epoch / Parameters.MaxEpoch));
                _adaptation = Math.Max(Parameters.MinAdaption, factor * Parameters.InitialAdaption);
                if (_radius > Parameters.MinRadius && epoch % Parameters.RadiusConstantTime == 0)
                {
                    _radius--;
                }

                //report
                /*if ( ReportOnIterationEndNeeded )
					OnIterationEnded( epoch, (double)epoch / (double)Parameters.MaxEpoch, "Iteration " + epoch + " finished.", true );
                else if (ReportOnProgressChangedNeeded)
                    OnProgressChanged( (double)epoch / (double)Parameters.MaxEpoch * 100 );*/
            }
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

        /// <summary>
        /// Rántsunk egyet az összes ponton.
        /// </summary>
        protected void Adjust(CancellationToken cancellationToken, System.Random rnd)
        {
            _tempPos = new Point
            {
                X = 0.1 * Parameters.Width + (rnd.NextDouble() * 0.8 * Parameters.Width),
                Y = 0.1 * Parameters.Height + (rnd.NextDouble() * 0.8 * Parameters.Height)
            };

            //get a random point in the container

            //find the closest vertex to this random point
            TVertex closest = GetClosest(_tempPos);

            //adjust the vertices to the selected vertex
            foreach (TVertex v in VisitedGraph.Vertices)
            {
                ISOMData vid = _isomDataDict[v];
                vid.Distance = 0;
                vid.Visited = false;
            }
            AdjustVertex(closest, cancellationToken);
        }

        private void AdjustVertex(TVertex closest, CancellationToken cancellationToken)
        {
            _queue.Clear();
            ISOMData vid = _isomDataDict[closest];
            vid.Distance = 0;
            vid.Visited = true;
            _queue.Enqueue(closest);

            while (_queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                TVertex current = _queue.Dequeue();
                ISOMData currentVid = _isomDataDict[current];
                Point pos = VertexPositions[current];

                Vector force = _tempPos - pos;
                double factor = _adaptation / Math.Pow(2, currentVid.Distance);

                pos += factor * force;
                VertexPositions[current] = pos;

                //ha még a hatókörön belül van
                if (currentVid.Distance < _radius)
                {
                    //akkor a szomszedokra is hatassal vagyunk
                    foreach (TVertex neighbour in VisitedGraph.GetNeighbours(current))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        ISOMData nvid = _isomDataDict[neighbour];
                        if (!nvid.Visited)
                        {
                            nvid.Visited = true;
                            nvid.Distance = currentVid.Distance + 1;
                            _queue.Enqueue(neighbour);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the the closest vertex to the given position.
        /// </summary>
        /// <param name="tempPos">The position.</param>
        /// <returns>Returns with the reference of the closest vertex.</returns>
        private TVertex GetClosest(Point tempPos)
        {
            TVertex vertex = default(TVertex);
            double distance = double.MaxValue;

            //find the closest vertex
            foreach (TVertex v in VisitedGraph.Vertices)
            {
                double d = (tempPos - VertexPositions[v]).Length;
                if (d < distance)
                {
                    vertex = v;
                    distance = d;
                }
            }
            return vertex;
        }

        private class ISOMData
        {
            public Vector Force = new Vector();
            public bool Visited;
            public double Distance;
        }
    }
}