using System;

using Windows.Foundation;

namespace GraphX.Controls
{
    public interface IEdgeLabelControl : IDisposable
    {
        /// <summary>
        /// Gets or sets if label should be aligned with the edge
        /// </summary>
        bool AlignToEdge { get; set; }

        /// <summary>
        /// Gets or sets label drawing angle in degrees
        /// </summary>
        double Angle { get; set; }

        /// <summary>
        /// Gets or sets label horizontal offset
        /// </summary>
        double LabelHorizontalOffset { get; set; }

        /// <summary>
        /// Gets or sets label vertical offset
        /// </summary>
        double LabelVerticalOffset { get; set; }

        /// <summary>
        /// Gets or sets if label is visible
        /// </summary>
        bool ShowLabel { get; set; }

        /// <summary>
        /// Get label rectangular size
        /// </summary>
        Rect GetSize();

        void Hide();

        void SetSize(Rect size);

        void Show();

        void UpdateLayout();

        /// <summary>
        /// Automaticaly update edge label position
        /// </summary>
        void UpdatePosition();

        /// <summary>
        /// Set label rectangular size
        /// </summary>
    }
}