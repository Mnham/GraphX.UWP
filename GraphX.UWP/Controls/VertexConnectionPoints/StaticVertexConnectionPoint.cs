using GraphX.Common.Enums;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    public class StaticVertexConnectionPoint : ContentControl, IVertexConnectionPoint
    {
        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            nameof(Shape),
            typeof(VertexShape),
            typeof(StaticVertexConnectionPoint),
            new PropertyMetadata(VertexShape.Circle));

        private Rect _rectangularSize;

        private VertexControl _vertexControl;

        /// <summary>
        /// Connector identifier
        /// </summary>
        public int Id { get; set; }

        public Rect RectangularSize
        {
            get
            {
                if (_rectangularSize == Rect.Empty)
                {
                    UpdateLayout();
                }
                return _rectangularSize;
            }
            private set => _rectangularSize = value;
        }

        /// <summary>
        /// Gets or sets shape form for connection point (affects math calculations for edge end placement)
        /// </summary>
        public VertexShape Shape
        {
            get => (VertexShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        protected VertexControl VertexControl => _vertexControl ??= GetVertexControl(GetParent());

        public StaticVertexConnectionPoint()
        {
            RenderTransformOrigin = new Point(.5, .5);
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            LayoutUpdated += OnLayoutUpdated;
        }

        public void Dispose()
        {
            _vertexControl = null;
        }

        public DependencyObject GetParent()
        {
            return Parent;
        }

        public void Hide()
        {
            SetValue(VisibilityProperty, Visibility.Collapsed);
        }

        public void Show()
        {
            SetValue(VisibilityProperty, Visibility.Visible);
        }

        public void Update()
        {
            UpdateLayout();
        }

        protected virtual void OnLayoutUpdated(object sender, object o)
        {
            Point position = TransformToVisual(VertexControl).TransformPoint(new Point());
            Point vPos = VertexControl.GetPosition();
            position = new Point(position.X + vPos.X, position.Y + vPos.Y);
            RectangularSize = new Rect(position, DesiredSize);
        }

        private static VertexControl GetVertexControl(DependencyObject parent)
        {
            while (parent != null)
            {
                if (parent is VertexControl control)
                {
                    return control;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}