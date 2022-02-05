using GraphX.Common.Enums;
using GraphX.Common.Exceptions;
using GraphX.Common.Interfaces;
using GraphX.Measure;

using QuikGraph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class RandomLayoutAlgorithm<TVertex, TEdge, TGraph> : LayoutAlgorithmBase<TVertex, TEdge, TGraph>
        where TVertex : class, IGraphXVertex
        where TEdge : IGraphXEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        private readonly RandomLayoutAlgorithmParams _parameters;

        public RandomLayoutAlgorithm(TGraph graph, IDictionary<TVertex, Point> positions, RandomLayoutAlgorithmParams prms)
            : base(graph, positions)
        {
            _parameters = prms;
        }

        public RandomLayoutAlgorithm(RandomLayoutAlgorithmParams prms)
            : base(default(TGraph), null)
        {
            _parameters = prms;
        }

        public override void Compute(CancellationToken cancellationToken)
        {
            VertexPositions.Clear();
            Rect bounds = _parameters == null ? new RandomLayoutAlgorithmParams().Bounds : _parameters.Bounds;
            int boundsWidth = (int)bounds.Width;
            int boundsHeight = (int)bounds.Height;
            int seed = _parameters == null ? Guid.NewGuid().GetHashCode() : _parameters.Seed;
            Random rnd = new Random(seed);
            foreach (TVertex item in VisitedGraph.Vertices)
            {
                if (item.SkipProcessing != ProcessingOptionEnum.Freeze || VertexPositions.Count == 0)
                {
                    int x = (int)bounds.X;
                    int y = (int)bounds.Y;
                    Size size = VertexSizes.FirstOrDefault(a => a.Key == item).Value;
                    VertexPositions.Add(item,
                        new Point(rnd.Next(x, x + boundsWidth - (int)size.Width),
                            rnd.Next(y, y + boundsHeight - (int)size.Height)));
                }
            }
        }

        public override bool NeedVertexSizes => true;

        public override bool SupportsObjectFreeze => true;

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