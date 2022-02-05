using GraphX.Measure;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.OverlapRemoval
{
    public class OverlapRemovalContext<TVertex> : IOverlapRemovalContext<TVertex>
    {
        public IDictionary<TVertex, Rect> Rectangles { get; private set; }

        public OverlapRemovalContext(IDictionary<TVertex, Rect> rectangles)
        {
            Rectangles = rectangles;
        }
    }
}