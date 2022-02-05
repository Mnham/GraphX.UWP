using GraphX.Common.Interfaces;
using GraphX.Measure;

using QuikGraph;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public interface ICompoundLayoutAlgorithm<TVertex, TEdge, out TGraph> : ILayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IBidirectionalGraph<TVertex, TEdge>
    {
        IDictionary<TVertex, Size> InnerCanvasSizes { get; }
    }
}