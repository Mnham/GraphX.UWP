using System;

using Windows.Foundation;

namespace GraphX.Controls.Models
{
    public sealed class VertexPositionEventArgs : EventArgs
    {
        /// <summary>
        /// Offset of the vertex control within the GraphArea
        /// </summary>
        public Point OffsetPosition { get; private set; }

        /// <summary>
        /// Attached coordinates X and Y
        /// </summary>
        public Point Position { get; private set; }

        /// <summary>
        /// Vertex control
        /// </summary>
        public VertexControlBase VertexControl { get; private set; }

        public VertexPositionEventArgs(Point offset, Point pos, VertexControlBase vc)
        {
            OffsetPosition = offset;
            VertexControl = vc;
            Position = pos;
        }
    }
}