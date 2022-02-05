using GraphX.Common.Enums;

using System;

using Windows.Foundation;
using Windows.UI.Xaml;

namespace GraphX.Controls
{
    public interface IVertexConnectionPoint : IDisposable
    {
        /// <summary>
        /// Connector identifier
        /// </summary>
        int Id { get; }

        Rect RectangularSize { get; }

        /// <summary>
        /// Gets or sets shape form for connection point (affects math calculations for edge end placement)
        /// </summary>
        VertexShape Shape { get; set; }

        DependencyObject GetParent();

        void Hide();

        void Show();

        void Update();
    }
}