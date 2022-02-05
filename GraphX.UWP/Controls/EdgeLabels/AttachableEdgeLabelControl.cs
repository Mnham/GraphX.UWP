using GraphX.Common.Exceptions;
using GraphX.Controls.Models;

using Windows.UI.Xaml;

namespace GraphX.Controls
{
    public class AttachableEdgeLabelControl : EdgeLabelControl, IAttachableControl<EdgeControl>
    {
        public static readonly DependencyProperty AttachNodeProperty = DependencyProperty.Register(
            nameof(AttachNode),
            typeof(EdgeControl),
            typeof(AttachableEdgeLabelControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets label attach node
        /// </summary>
        public EdgeControl AttachNode
        {
            get => (EdgeControl)GetValue(AttachNodeProperty);
            private set => SetValue(AttachNodeProperty, value);
        }

        public AttachableEdgeLabelControl()
        {
            DataContext = this;
            DefaultStyleKey = typeof(AttachableEdgeLabelControl);
        }

        /// <summary>
        /// Attach label to VertexControl
        /// </summary>
        /// <param name="node">VertexControl node</param>
        public virtual void Attach(EdgeControl node)
        {
            AttachNode = node;
            node.AttachLabel(this);
        }

        /// <summary>
        /// Detach label from control
        /// </summary>
        public virtual void Detach()
        {
            AttachNode = null;
        }

        protected override EdgeControl GetEdgeControl(DependencyObject parent)
        {
            return AttachNode ?? throw new GX_InvalidDataException("AttachableEdgeLabelControl node is not attached!");
        }
    }
}