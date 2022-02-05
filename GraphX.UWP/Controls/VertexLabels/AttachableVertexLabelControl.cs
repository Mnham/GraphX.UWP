using GraphX.Common.Exceptions;
using GraphX.Controls.Models;

using System.ComponentModel;

using Windows.Foundation;
using Windows.UI.Xaml;

namespace GraphX.Controls
{
    public class AttachableVertexLabelControl : VertexLabelControl, IAttachableControl<VertexControl>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty AttachNodeProperty = DependencyProperty.Register(
            nameof(AttachNode),
            typeof(VertexControl),
            typeof(AttachableVertexLabelControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets label attach node
        /// </summary>
        public VertexControl AttachNode
        {
            get => (VertexControl)GetValue(AttachNodeProperty);
            private set
            {
                SetValue(AttachNodeProperty, value);
                OnPropertyChanged(nameof(AttachNode));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AttachableVertexLabelControl()
        {
            DataContext = this;
            DefaultStyleKey = typeof(AttachableVertexLabelControl);
        }

        /// <summary>
        /// Attach label to VertexControl
        /// </summary>
        /// <param name="node">VertexControl node</param>
        public virtual void Attach(VertexControl node)
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

        public override void UpdatePosition()
        {
            if (double.IsNaN(DesiredSize.Width) || DesiredSize.Width == 0)
            {
                return;
            }

            VertexControl vc = GetVertexControl(GetParent());
            if (vc == null)
            {
                return;
            }

            if (LabelPositionMode == VertexLabelPositionMode.Sides)
            {
                Point vcPos = vc.GetPosition();
                Point pt;
                switch (LabelPositionSide)
                {
                    case VertexLabelPositionSide.TopRight:
                        pt = new Point(vcPos.X + vc.DesiredSize.Width, vcPos.Y + -DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.BottomRight:
                        pt = new Point(vcPos.X + vc.DesiredSize.Width, vcPos.Y + vc.DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.TopLeft:
                        pt = new Point(vcPos.X + -DesiredSize.Width, vcPos.Y + -DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.BottomLeft:
                        pt = new Point(vcPos.X + -DesiredSize.Width, vcPos.Y + vc.DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.Top:
                        pt = new Point(vcPos.X + vc.DesiredSize.Width * .5 - DesiredSize.Width * .5, vcPos.Y + -DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.Bottom:
                        pt = new Point(vcPos.X + vc.DesiredSize.Width * .5 - DesiredSize.Width * .5, vcPos.Y + vc.DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.Left:
                        pt = new Point(vcPos.X + -DesiredSize.Width, vcPos.Y + vc.DesiredSize.Height * .5f - DesiredSize.Height * .5);
                        break;

                    case VertexLabelPositionSide.Right:
                        pt = new Point(vcPos.X + vc.DesiredSize.Width, vcPos.Y + vc.DesiredSize.Height * .5f - DesiredSize.Height * .5);
                        break;

                    default:
                        throw new GX_InvalidDataException("UpdatePosition() -> Unknown vertex label side!");
                }
                LastKnownRectSize = new Rect(pt, DesiredSize);
            }
            else
            {
                LastKnownRectSize = new Rect(LabelPosition, DesiredSize);
            }

            Arrange(LastKnownRectSize);
        }

        protected override VertexControl GetVertexControl(DependencyObject parent)
        {
            //if(AttachNode == null)
            //    throw new GX_InvalidDataException("AttachableVertexLabelControl node is not attached!");
            return AttachNode;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}