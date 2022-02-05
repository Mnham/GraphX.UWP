using GraphX.Measure;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.OverlapRemoval
{
    public interface IOverlapRemovalContext<TVertex>
    {
        IDictionary<TVertex, Rect> Rectangles { get; }
    }
}