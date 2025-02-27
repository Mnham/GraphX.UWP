﻿using QuikGraph;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public interface ILayoutInfoIterationEventArgs<TVertex, in TEdge>
        : ILayoutIterationEventArgs<TVertex>
        where TVertex : class
        where TEdge : IEdge<TVertex>
    {
        object GetVertexInfo(TVertex vertex);

        object GetEdgeInfo(TEdge edge);
    }

    public interface ILayoutInfoIterationEventArgs<TVertex, TEdge, TVertexInfo, TEdgeInfo>
        : ILayoutInfoIterationEventArgs<TVertex, TEdge>
        where TVertex : class
        where TEdge : IEdge<TVertex>
    {
        IDictionary<TVertex, TVertexInfo> VertexInfos { get; }
        IDictionary<TEdge, TEdgeInfo> EdgeInfos { get; }
    }
}