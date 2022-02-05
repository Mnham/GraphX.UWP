using GraphX.Common.Exceptions;

using QuikGraph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public partial class EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        private class WHOptimizationLayerInfo
        {
            public bool IsInsertedLayer;
            public double LayerWidth;
            public double LayerHeight;
            public readonly Queue<WHOptimizationVertexInfo> Vertices = new Queue<WHOptimizationVertexInfo>();
        }

        private class WHOptimizationVertexInfo
        {
            public SugiVertex Vertex;
            public double Value;
            public double Cost;

            public double ValuePerCost
            {
                get
                {
                    if (Value < 0)
                    {
                        return double.NaN;
                    }
                    else if (Cost <= 0)
                    {
                        return double.PositiveInfinity;
                    }
                    else
                    {
                        return Value / Cost;
                    }
                }
            }
        }

        private double _whAverageVertexHeight;
        private double _actualWidth;
        private double _actualHeight;

        private double _actualWidthPerHeight => _actualWidth / _actualHeight;

        private readonly IList<WHOptimizationLayerInfo> _whOptLayerInfos =
            new List<WHOptimizationLayerInfo>();

        private readonly IDictionary<SugiVertex, WHOptimizationVertexInfo> _whOptVertexInfos =
            new Dictionary<SugiVertex, WHOptimizationVertexInfo>();

        /// <summary>
        /// From the original graph it creates a sparse normalized graph
        /// with segments and dummy vertices (p-vertex, q-vertex, s-vertex).
        /// </summary>
        private void BuildSparseNormalizedGraph(CancellationToken cancellationToken)
        {
            CreateInitialLayering(cancellationToken);
            if (Parameters.OptimizeWidth)
            {
                DoWidthAndHeightOptimization(cancellationToken);
            }

            CreateDummyVerticesAndSegments();
            RemoveParallelEdges(cancellationToken);
        }

        private void DoWidthAndHeightOptimization(CancellationToken cancellationToken)
        {
            CreateVertexWHOptInfos();
            CreateLayerWHOptInfos(cancellationToken);

            if (_actualWidthPerHeight <= Parameters.WidthPerHeight)
            {
                return;
            }

            bool optimized;
            do
            {
                optimized = DoWHOptimizationStep();
            } while (optimized);
            RewriteLayerIndexes(cancellationToken);
        }

        private void RewriteLayerIndexes(CancellationToken cancellationToken)
        {
            int i = 0;
            foreach (IList<SugiVertex> layer in _layers)
            {
                foreach (SugiVertex vertex in layer)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    vertex.LayerIndex = i;
                }
                i++;
            }
        }

        private bool DoWHOptimizationStep()
        {
            double desiredWidth = _actualHeight * Parameters.WidthPerHeight;

            int maxWidthLayerIndex = 0;
            WHOptimizationLayerInfo maxWidthLayer = _whOptLayerInfos[0];
            for (int i = 0; i < _whOptLayerInfos.Count; i++)
            {
                if (_whOptLayerInfos[i].LayerWidth > maxWidthLayer.LayerWidth
                    && _whOptLayerInfos[i].Vertices.Count > 0
                    && _whOptLayerInfos[i].LayerWidth > desiredWidth)
                {
                    maxWidthLayer = _whOptLayerInfos[i];
                    maxWidthLayerIndex = i;
                }
            }

            if (maxWidthLayer.LayerWidth <= desiredWidth || maxWidthLayer.Vertices.Count <= 0)
            {
                return false;
            }

            //get a layer nearby the maxWidthLayer
            int insertedLayerIndex = -1;
            WHOptimizationLayerInfo insertedLayerInfo = null;
            IList<SugiVertex> insertedLayer = null;
            if (maxWidthLayerIndex < _whOptLayerInfos.Count - 1
                && _whOptLayerInfos[maxWidthLayerIndex + 1].IsInsertedLayer
                && _whOptLayerInfos[maxWidthLayerIndex + 1].LayerWidth < (desiredWidth - maxWidthLayer.Vertices.Peek().Cost))
            {
                insertedLayerIndex = maxWidthLayerIndex + 1;
                insertedLayerInfo = _whOptLayerInfos[insertedLayerIndex];
                insertedLayer = _layers[insertedLayerIndex];
            }
            else
            {
                //insert a new layer
                insertedLayerIndex = maxWidthLayerIndex + 1;
                double width = 0;
                double c = 0;
                if (insertedLayerIndex > 0)
                {
                    foreach (SugiVertex vertex in _layers[insertedLayerIndex - 1])
                    {
                        width += Math.Max(0, _graph.OutDegree(vertex) - 1) * Parameters.LayerDistance;
                    }
                    c += 1;
                }
                if (insertedLayerIndex < _layers.Count - 1)
                {
                    foreach (SugiVertex vertex in _layers[insertedLayerIndex])
                    {
                        width += Math.Max(0, _graph.OutDegree(vertex) - 1) * Parameters.LayerDistance;
                    }
                    c += 1;
                }
                if (c > 0)
                {
                    width /= c;
                }

                if (width >= (desiredWidth - _whOptLayerInfos[insertedLayerIndex - 1].Vertices.Peek().Cost))
                {
                    return false;
                }

                insertedLayerInfo = new WHOptimizationLayerInfo
                {
                    LayerWidth = width
                };
                insertedLayer = new List<SugiVertex>();
                _whOptLayerInfos.Insert(insertedLayerIndex, insertedLayerInfo);
                _layers.Insert(insertedLayerIndex, insertedLayer);

                double height = 0.0;
                while (insertedLayerInfo.LayerWidth < _whOptLayerInfos[insertedLayerIndex - 1].LayerWidth
                    && _whOptLayerInfos[insertedLayerIndex - 1].Vertices.Count > 0
                    && insertedLayerInfo.LayerWidth <= (desiredWidth - _whOptLayerInfos[insertedLayerIndex - 1].Vertices.Peek().Cost))
                {
                    WHOptimizationVertexInfo repositionedVertex = _whOptLayerInfos[insertedLayerIndex - 1].Vertices.Dequeue();
                    insertedLayerInfo.LayerWidth += repositionedVertex.Cost;
                    _whOptLayerInfos[insertedLayerIndex - 1].LayerWidth -= repositionedVertex.Value;
                    _layers[insertedLayerIndex - 1].Remove(repositionedVertex.Vertex);
                    insertedLayer.Add(repositionedVertex.Vertex);
                    height = Math.Max(height, repositionedVertex.Vertex.Size.Height);
                }
                _actualHeight += height + Parameters.LayerDistance;
                _actualWidth = _whOptLayerInfos.Max(li => li.LayerWidth);
            }

            return true;
        }

        private void CreateLayerWHOptInfos(CancellationToken cancellationToken)
        {
            _actualHeight = 0;
            _actualWidth = 0;
            foreach (IList<SugiVertex> layer in _layers)
            {
                WHOptimizationLayerInfo layerInfo = new WHOptimizationLayerInfo
                {
                    IsInsertedLayer = false
                };
                foreach (SugiVertex vertex in layer)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    layerInfo.LayerHeight = Math.Max(vertex.Size.Height, layerInfo.LayerHeight);
                    layerInfo.LayerWidth += vertex.Size.Width;
                    if (_whOptVertexInfos.TryGetValue(vertex, out WHOptimizationVertexInfo vertexInfo))
                    {
                        if (vertexInfo.ValuePerCost >= 0)
                        {
                            layerInfo.Vertices.Enqueue(vertexInfo);
                        }
                    }
                }
                layerInfo.LayerWidth += Math.Max(0, layer.Count - 1) * Parameters.VertexDistance;
                _actualWidth = Math.Max(layerInfo.LayerWidth, _actualWidth);
                List<WHOptimizationVertexInfo> vertexList = new List<WHOptimizationVertexInfo>();
                foreach (WHOptimizationVertexInfo v in layerInfo.Vertices)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!double.IsNaN(v.ValuePerCost) && !double.IsPositiveInfinity(v.ValuePerCost) && !double.IsNegativeInfinity(v.ValuePerCost))
                    {
                        vertexList.Add(v);
                    }
                }
                vertexList.Sort(new Comparison<WHOptimizationVertexInfo>(
                    (v1, v2) => Math.Sign(v2.ValuePerCost - v1.ValuePerCost)));
                _actualHeight += layerInfo.LayerHeight + Parameters.LayerDistance;
                layerInfo.Vertices.Clear();
                foreach (WHOptimizationVertexInfo v in vertexList)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    layerInfo.Vertices.Enqueue(v);
                }
                _whOptLayerInfos.Add(layerInfo);
            }
            _actualHeight -= Parameters.LayerDistance;
            _actualWidth -= Parameters.VertexDistance;
        }

        private void CreateVertexWHOptInfos()
        {
            double vertexCount = 0;
            foreach (SugiVertex vertex in _graph.Vertices)
            {
                if (vertex.Type != VertexTypes.Original)
                {
                    continue;
                }

                WHOptimizationVertexInfo whOptInfo = new WHOptimizationVertexInfo
                {
                    Value = vertex.Size.Width - Math.Max(0, _graph.InDegree(vertex) - 1) * Parameters.VertexDistance,
                    Cost = vertex.Size.Width - Math.Max(0, _graph.OutDegree(vertex) - 1) * Parameters.VertexDistance,
                    Vertex = vertex
                };
                _whOptVertexInfos[vertex] = whOptInfo;
                vertexCount++;
                _whAverageVertexHeight += vertex.Size.Height;
            }
            _whAverageVertexHeight /= vertexCount;
        }

        private void RemoveParallelEdges(CancellationToken cancellationToken)
        {
            foreach (SugiVertex vertex in _graph.Vertices)
            {
                HashSet<SugiVertex> targets = new HashSet<SugiVertex>();
                foreach (SugiEdge edge in _graph.OutEdges(vertex).ToArray())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (targets.Contains(edge.Target))
                    {
                        _graph.RemoveEdge(edge);
                    }
                }
            }
        }

        private void CreateInitialLayering(CancellationToken cancellationToken)
        {
            LayeredTopologicalSortAlgorithm<SugiVertex, SugiEdge> lts = new LayeredTopologicalSortAlgorithm<SugiVertex, SugiEdge>(_graph);
            lts.Compute();

            for (int i = 0; i < lts.LayerCount; i++)
            {
                //set the layer
                _layers.Add(lts.Layers[i].ToList());

                //assign the layerindex
                foreach (SugiVertex v in _layers[i])
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    v.LayerIndex = i;
                }
            }

            //minimize edge length
            if (Parameters.MinimizeEdgeLength)
            {
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    IList<SugiVertex> layer = _layers[i];
                    foreach (SugiVertex v in layer.ToList())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (_graph.OutDegree(v) == 0)
                        {
                            continue;
                        }

                        //put the vertex above the descendant on the highest layer
                        int newLayerIndex = _graph.OutEdges(v).Min(edge => edge.Target.LayerIndex - 1);

                        if (newLayerIndex != v.LayerIndex)
                        {
                            //we're changing layer
                            layer.Remove(v);
                            _layers[newLayerIndex].Add(v);
                            v.LayerIndex = newLayerIndex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replace long edge (span(edge) > 1) with dummy vertices
        /// or segments (span(edge) > 2).
        /// </summary>
        private void CreateDummyVerticesAndSegments()
        {
            foreach (SugiEdge edge in _graph.Edges.ToList())
            {
                int sourceLayerIndex = edge.Source.LayerIndex;
                int targetLayerIndex = edge.Target.LayerIndex;
                int span = targetLayerIndex - sourceLayerIndex;
                if (span < 1)
                {
                    throw new ArgumentException("span cannot be lower than 1");
                }

                if (span == 1)
                {
                    continue;
                }

                _graph.RemoveEdge(edge);
                bool notReversed = edge.Source.OriginalVertex == edge.OriginalEdge.Source && edge.Target.OriginalVertex == edge.OriginalEdge.Target;
                List<SugiVertex> dummyVertexList = new List<SugiVertex>();
                _dummyVerticesOfEdges[edge.OriginalEdge] = dummyVertexList;
                if (span == 2)
                {
                    //insert an R-vertex
                    SugiVertex rVertex = AddDummyVertex(VertexTypes.RVertex, sourceLayerIndex + 1);
                    _graph.AddEdge(new SugiEdge(edge.OriginalEdge, edge.Source, rVertex));
                    _graph.AddEdge(new SugiEdge(edge.OriginalEdge, rVertex, edge.Target));
                    dummyVertexList.Add(rVertex);
                }
                else
                {
                    //insert a P-vertex, a Q-vertex and a Segment
                    SugiVertex pVertex = AddDummyVertex(VertexTypes.PVertex, sourceLayerIndex + 1);
                    SugiVertex qVertex = AddDummyVertex(VertexTypes.QVertex, targetLayerIndex - 1);
                    if (notReversed)
                    {
                        dummyVertexList.Add(pVertex);
                        dummyVertexList.Add(qVertex);
                    }
                    else
                    {
                        dummyVertexList.Add(qVertex);
                        dummyVertexList.Add(pVertex);
                    }
                    _graph.AddEdge(new SugiEdge(edge.OriginalEdge, edge.Source, pVertex));
                    _graph.AddEdge(new SugiEdge(edge.OriginalEdge, qVertex, edge.Target));
                    AddSegment(pVertex, qVertex, edge);
                }
            }
        }

        /// <summary>
        /// Adds a new segment to the sparse compaction graph.
        /// </summary>
        /// <param name="pVertex">The source vertex of the segment.</param>
        /// <param name="qVertex">The target vertex of the segment.</param>
        /// <param name="edge">The edge which has been replaced by the
        /// dummy vertices and this segment.</param>
        /// <returns>The newly created segment.</returns>
        private Segment AddSegment(SugiVertex pVertex, SugiVertex qVertex, SugiEdge edge)
        {
            Segment segment = new Segment()
            {
                PVertex = pVertex,
                QVertex = qVertex
            };
            pVertex.Segment = segment;
            qVertex.Segment = segment;
            return segment;
        }

        /// <summary>
        /// Adds a dummy vertex to the sparse compaction graph.
        /// </summary>
        /// <param name="type">The type of the dummy vertex (p,q,r).</param>
        /// <param name="layerIndex">The index of the layer of the vertex.</param>
        /// <returns>The new vertex which has been added to the graph and the
        /// layers.</returns>
        private SugiVertex AddDummyVertex(VertexTypes type, int layerIndex)
        {
            SugiVertex vertex = new SugiVertex()
            {
                Type = type,
                LayerIndex = layerIndex
            };
            _layers[layerIndex].Add(vertex);
            _graph.AddVertex(vertex);

            return vertex;
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
    }
}