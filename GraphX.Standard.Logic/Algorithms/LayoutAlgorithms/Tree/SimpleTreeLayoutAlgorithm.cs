﻿using GraphX.Common.Exceptions;
using GraphX.Measure;

using QuikGraph;
using QuikGraph.Algorithms.Search;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public partial class SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph> : DefaultParameterizedLayoutAlgorithmBase<TVertex, TEdge, TGraph, SimpleTreeLayoutParameters>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        protected BidirectionalGraph<TVertex, Edge<TVertex>> SpanningTree;
        protected IDictionary<TVertex, Size> Sizes;
        protected readonly IDictionary<TVertex, VertexData> Data = new Dictionary<TVertex, VertexData>();
        protected readonly IList<Layer> Layers = new List<Layer>();
        private int _direction;

        public SimpleTreeLayoutAlgorithm(TGraph visitedGraph, IDictionary<TVertex, Point> vertexPositions, IDictionary<TVertex, Size> vertexSizes, SimpleTreeLayoutParameters parameters)
            : base(visitedGraph, vertexPositions, parameters)
        {
            VertexSizes = vertexSizes == null ? new Dictionary<TVertex, Size>() : new Dictionary<TVertex, Size>(vertexSizes);
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            Sizes = new Dictionary<TVertex, Size>(VertexSizes);
            if (Parameters.Direction == LayoutDirection.LeftToRight || Parameters.Direction == LayoutDirection.RightToLeft)
            {
                //change the sizes
                foreach (KeyValuePair<TVertex, Size> sizePair in Sizes.ToArray())
                {
                    Sizes[sizePair.Key] = new Size(sizePair.Value.Height, sizePair.Value.Width);
                }
            }

            if (Parameters.Direction == LayoutDirection.RightToLeft || Parameters.Direction == LayoutDirection.BottomToTop)
            {
                _direction = -1;
            }
            else
            {
                _direction = 1;
            }

            GenerateSpanningTree(cancellationToken);
            //DoWidthAndHeightOptimization();

            UndirectedBidirectionalGraph<TVertex, TEdge> graph = new UndirectedBidirectionalGraph<TVertex, TEdge>(VisitedGraph);
            QuikGraph.Algorithms.ConnectedComponents.ConnectedComponentsAlgorithm<TVertex, TEdge> scca = new QuikGraph.Algorithms.ConnectedComponents.ConnectedComponentsAlgorithm<TVertex, TEdge>(graph);
            scca.Compute();

            // Order connected components by their vertices count
            // Group vertices by connected component (they should be placed together)
            // Order vertices inside each conected component by in degree first, then out dregee
            // (roots should be placed in the first layer and leafs in the last layer)
            IOrderedEnumerable<IGrouping<int, TVertex>> components = from e in scca.Components
                                                                     group e.Key by e.Value into c
                                                                     orderby c.Count() descending
                                                                     select c;

            foreach (IGrouping<int, TVertex> c in components)
            {
                bool firstOfComponent = true;
                IOrderedEnumerable<TVertex> vertices = from v in c
                                                       orderby VisitedGraph.InDegree(v), VisitedGraph.OutDegree(v) descending
                                                       select v;

                foreach (TVertex source in vertices)
                {
                    CalculatePosition(source, null, 0, firstOfComponent);

                    if (firstOfComponent)
                    {
                        firstOfComponent = false;
                    }
                }
            }

            AssignPositions(cancellationToken);
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

        protected virtual void GenerateSpanningTree(CancellationToken cancellationToken)
        {
            SpanningTree = new BidirectionalGraph<TVertex, Edge<TVertex>>(false);
            SpanningTree.AddVertexRange(VisitedGraph.Vertices.OrderBy(v => VisitedGraph.InDegree(v)));

            EdgeAction<TVertex, TEdge> action = e =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                SpanningTree.AddEdge(new Edge<TVertex>(e.Source, e.Target));
            };

            switch (Parameters.SpanningTreeGeneration)
            {
                case SpanningTreeGeneration.BFS:
                    BreadthFirstSearchAlgorithm<TVertex, TEdge> bfsAlgo = new BreadthFirstSearchAlgorithm<TVertex, TEdge>(VisitedGraph);
                    bfsAlgo.TreeEdge += action;
                    bfsAlgo.Compute();
                    break;

                case SpanningTreeGeneration.DFS:
                    DepthFirstSearchAlgorithm<TVertex, TEdge> dfsAlgo = new DepthFirstSearchAlgorithm<TVertex, TEdge>(VisitedGraph);
                    dfsAlgo.TreeEdge += action;
                    dfsAlgo.ForwardOrCrossEdge += action;
                    dfsAlgo.Compute();
                    break;
            }
        }

        protected virtual double CalculatePosition(TVertex v, TVertex parent, int l, bool firstOfComponent)
        {
            if (Data.ContainsKey(v))
            {
                return -1; //this vertex is already layed out
            }

            while (l >= Layers.Count)
            {
                Layers.Add(new Layer());
            }

            Layer layer = Layers[l];
            Size size = Sizes[v];
            VertexData d = new VertexData { Parent = parent };
            Data[v] = d;

            layer.NextPosition += size.Width / 2.0;
            if (l > 0)
            {
                layer.NextPosition += Layers[l - 1].LastTranslate;
                Layers[l - 1].LastTranslate = 0;
            }

            if (firstOfComponent)
            {
                layer.NextPosition += Parameters.ComponentGap;
            }

            layer.Size = Math.Max(layer.Size, size.Height);
            layer.Vertices.Add(v);
            if (SpanningTree.OutDegree(v) == 0)
            {
                d.Position = layer.NextPosition;
            }
            else
            {
                double minPos = double.MaxValue;
                double maxPos = -double.MaxValue;
                //first put the children
                foreach (TVertex child in SpanningTree.OutEdges(v).Select(e => e.Target))
                {
                    double childPos = CalculatePosition(child, v, l + 1, firstOfComponent);

                    if (childPos >= 0)
                    {
                        minPos = Math.Min(minPos, childPos);
                        maxPos = Math.Max(maxPos, childPos);
                    }

                    if (firstOfComponent)
                    {
                        firstOfComponent = false;
                    }
                }
                if (minPos != double.MaxValue)
                {
                    d.Position = (minPos + maxPos) / 2.0;
                }
                else
                {
                    d.Position = layer.NextPosition;
                }

                d.Translate = Math.Max(layer.NextPosition - d.Position, 0);

                layer.LastTranslate = d.Translate;
                d.Position += d.Translate;
                layer.NextPosition = d.Position;
            }
            layer.NextPosition += size.Width / 2.0 + Parameters.VertexGap;

            return d.Position;
        }

        protected virtual void AssignPositions(CancellationToken cancellationToken)
        {
            double layerSize = 0;
            bool changeCoordinates = (Parameters.Direction == LayoutDirection.LeftToRight || Parameters.Direction == LayoutDirection.RightToLeft);

            foreach (Layer layer in Layers)
            {
                foreach (TVertex v in layer.Vertices)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Size size = Sizes[v];
                    VertexData d = Data[v];
                    if (d.Parent != null)
                    {
                        d.Position += Data[d.Parent].Translate;
                        d.Translate += Data[d.Parent].Translate;
                    }

                    double x = d.Position - size.Width / 2.0;
                    double y = _direction * (layerSize + (layer.Size - size.Height) / 2.0);
                    Point pos = changeCoordinates ? new Point(y, x) : new Point(x, y);

                    VertexPositions[v] = pos;
                }

                layerSize += layer.Size + Parameters.LayerGap;
            }

            if (_direction < 0)
            {
                NormalizePositions();
            }
        }
    }
}