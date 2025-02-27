﻿using GraphX.Measure;

using QuikGraph;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class TestingCompoundLayoutIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo>
        : CompoundLayoutIterationEventArgs<TVertex, TEdge>, ILayoutInfoIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo>
        where TVertex : class
        where TEdge : IEdge<TVertex>
    {
        private readonly IDictionary<TVertex, TVertexInfo> _vertexInfos;

        public Point GravitationCenter { get; private set; }

        public TestingCompoundLayoutIterationEventArgs(
            int iteration,
            double statusInPercent,
            string message,
            IDictionary<TVertex, Point> vertexPositions,
            IDictionary<TVertex, Size> innerCanvasSizes,
            IDictionary<TVertex, TVertexInfo> vertexInfos,
            Point gravitationCenter)
            : base(iteration, statusInPercent, message, vertexPositions, innerCanvasSizes)
        {
            _vertexInfos = vertexInfos;
            GravitationCenter = gravitationCenter;
        }

        public override object GetVertexInfo(TVertex vertex)
        {
            if (_vertexInfos.TryGetValue(vertex, out TVertexInfo info))
            {
                return info;
            }

            return null;
        }

        public IDictionary<TVertex, TVertexInfo> VertexInfos => _vertexInfos;

        public IDictionary<TEdge, TEdgeInfo> EdgeInfos => null;
    }
}