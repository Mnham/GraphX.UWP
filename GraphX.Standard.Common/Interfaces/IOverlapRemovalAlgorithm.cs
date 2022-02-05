using GraphX.Measure;

using System.Collections.Generic;

namespace GraphX.Common.Interfaces
{
    public interface IOverlapRemovalAlgorithm<TObject> : IExternalOverlapRemoval<TObject>
    {
        IOverlapRemovalParameters GetParameters();

        /// <summary>
        /// Initialize algorithm initial data
        /// </summary>
        /// <param name="rectangles">Size rectangles</param>
        void Initialize(IDictionary<TObject, Rect> rectangles);
    }

    public interface IOverlapRemovalAlgorithm<TObject, TParam> : IOverlapRemovalAlgorithm<TObject>
        where TParam : IOverlapRemovalParameters
    {
        /// <summary>
        /// Algorithm parameters
        /// </summary>
		TParam Parameters { get; }

        /// <summary>
        /// Initialize algorithm initial data
        /// </summary>
        /// <param name="rectangles">Size rectangles</param>
        /// <param name="parameters">algorithm parameters</param>
	    void Initialize(IDictionary<TObject, Rect> rectangles, TParam parameters);
    }
}