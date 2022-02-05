using GraphX.Measure;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public interface ICompoundLayoutIterationEventArgs<TVertex>
        : ILayoutIterationEventArgs<TVertex>
        where TVertex : class
    {
        IDictionary<TVertex, Size> InnerCanvasSizes { get; }
    }
}