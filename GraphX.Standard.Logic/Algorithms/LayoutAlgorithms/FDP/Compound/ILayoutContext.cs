using GraphX.Measure;

using QuikGraph;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public interface ILayoutContext<TVertex, TEdge, out TGraph>
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        IDictionary<TVertex, Point> Positions { get; }
        IDictionary<TVertex, Size> Sizes { get; }

        TGraph Graph { get; }

        LayoutMode Mode { get; }
    }
}