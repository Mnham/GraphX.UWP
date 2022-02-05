using System;

using Windows.Foundation;
using Windows.UI.Xaml;

using Vector = GraphX.Measure.Vector;

namespace GraphX.Controls
{
    public interface IEdgePointer : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the pointer is suppressed. A suppressed pointer won't be displayed, but
        /// suppressing does not alter the underlying Visibility property value.
        /// </summary>
        bool IsSuppressed { get; }

        /// <summary>
        /// Gets or sets if image has to be rotated according to edge directions
        /// </summary>
        bool NeedRotation { get; }

        /// <summary>
        /// Gets is control visible
        /// </summary>
        Visibility Visibility { get; }

        /// <summary>
        /// Returns edge pointer center position coordinates
        /// </summary>
        Point GetPosition();

        void Hide();

        void SetManualPosition(Point position);

        void Show();

        /// <summary>
        /// Suppresses the pointer display without altering the underlying Visibility property value.
        /// </summary>
        void Suppress();

        /// <summary>
        /// Removes pointer display suppression, restoring the pointer to its underlying Visibility property value. If Visibility
        /// was set to Hidden or Collapsed, the pointer will remain invisible to the user, but if the Visibility base value
        /// is Visible, it should appear again.
        /// </summary>
        void UnSuppress();

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        Point Update(Point? position, Vector direction, double angle = 0d);
    }
}