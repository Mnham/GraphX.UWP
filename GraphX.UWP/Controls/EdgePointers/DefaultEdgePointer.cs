using GraphX.Measure;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using DefaultEventArgs = System.Object;
using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;

namespace GraphX.Controls
{
    /// <summary>
    /// Edge pointer control for edge endpoints customization
    /// Represents ContentControl that can host different content, e.g. Image or Path
    /// </summary>
    public class DefaultEdgePointer : ContentControl, IEdgePointer
    {
        public static readonly DependencyProperty IsSuppressedProperty = IsSuppressedPropertyKey;

        public static readonly DependencyProperty NeedRotationProperty = DependencyProperty.Register(
            nameof(NeedRotation),
            typeof(bool),
            typeof(EdgeControl),
            new PropertyMetadata(true));

        internal Rect LastKnownRectSize;

        private static readonly DependencyProperty IsSuppressedPropertyKey = DependencyProperty.Register(
            nameof(IsSuppressed),
            typeof(bool),
            typeof(DefaultEdgePointer),
            new PropertyMetadata(false, OnSuppressChanged));

        private EdgeControl _edgeControl;

        /// <summary>
        /// Gets a value indicating whether the pointer is suppressed. A suppressed pointer won't be displayed, but
        /// suppressing does not alter the underlying Visibility property value.
        /// </summary>
        public bool IsSuppressed
        {
            get => (bool)GetValue(IsSuppressedProperty);
            private set => SetValue(IsSuppressedPropertyKey, value);
        }

        /// <inheritdoc />
        public bool NeedRotation
        {
            get => (bool)GetValue(NeedRotationProperty);
            set => SetValue(NeedRotationProperty, value);
        }

        /// <summary>
        /// Gets or sets offset for the image position
        /// </summary>
        public Point Offset { get; set; }

        protected EdgeControl EdgeControl => _edgeControl ??= GetEdgeControl(GetParent());

        /// <summary>
        /// This static initializer is used to override PropertyMetadata of the Visibility property so that it
        /// can be coerced according to the IsSuppressed property value. Suppressing an edge pointer will make
        /// it invisible to the user without altering the underlying value of the Visibility property. Thus,
        /// visibility can be controlled independently of other factors that may require making the pointer
        /// invisible to the user. For example, the HideEdgePointerByEdgeLength feature of EdgeControlBase may
        /// need to ensure the pointer is removed from view, but when the constraint is removed, it shouldn't
        /// cause pointers to be shown that weren't shown before.
        /// </summary>
        static DefaultEdgePointer() { }

        public DefaultEdgePointer()
        {
            RenderTransformOrigin = new Point(.5, .5);
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            LayoutUpdated += EdgePointer_LayoutUpdated;
        }

        public void Dispose()
        {
            _edgeControl = null;
        }

        /// <inheritdoc />
        public Point GetPosition()
        {
            return LastKnownRectSize.IsEmpty ? new Point() : LastKnownRectSize.Center();
        }

        public void Hide()
        {
            SetValue(VisibilityProperty, Visibility.Collapsed);
        }

        public void SetManualPosition(Point position)
        {
            LastKnownRectSize = new Rect(new Point(position.X - (DesiredSize.Width * .5), position.Y - (DesiredSize.Height * .5)), DesiredSize);
            Arrange(LastKnownRectSize);
        }

        public void Show()
        {
            SetValue(VisibilityProperty, Visibility.Visible);
        }

        /// <summary>
        /// Suppresses the pointer from view, overriding the Visibility value until unsuppressed.
        /// </summary>
        public void Suppress()
        {
            IsSuppressed = true;
        }

        /// <summary>
        /// Removes the suppression constraint, returning to the base value of the Visibility property.
        /// </summary>
        public void UnSuppress()
        {
            IsSuppressed = false;
        }

        /// <summary>
        /// Update edge pointer position and angle
        /// </summary>
        public virtual Point Update(Point? position, Vector direction, double angle = 0d)
        {
            //var vecOffset = new Vector(direction.X * Offset.X, direction.Y * Offset.Y);
            if (DesiredSize.Width == 0 || DesiredSize.Height == 0 || !position.HasValue)
            {
                return new Point();
            }

            Vector vecMove = new(direction.X * DesiredSize.Width * .5, direction.Y * DesiredSize.Height * .5);
            position = new Point(position.Value.X - vecMove.X, position.Value.Y - vecMove.Y);// + vecOffset;
            if (!double.IsNaN(DesiredSize.Width) && DesiredSize.Width != 0 && !double.IsNaN(position.Value.X))
            {
                LastKnownRectSize = new Rect(new Point(position.Value.X - (DesiredSize.Width * .5), position.Value.Y - (DesiredSize.Height * .5)), DesiredSize);
                Arrange(LastKnownRectSize);
            }

            try
            {
                if (NeedRotation)
                {
                    RenderTransform = new RotateTransform { Angle = double.IsNaN(angle) ? 0 : angle, CenterX = 0, CenterY = 0 };
                }
            }
            catch
            {
                //TODO ex handling and reason
            }

            return new Point(direction.X * ActualWidth, direction.Y * ActualHeight);
        }

        /// <summary>
        /// This coercion callback is used to alter the effective value of Visibility when pointer suppression is in effect.
        /// When the suppression constraint is removed, the base value of Visibility becomes effective again.
        /// </summary>
        private static object CoerceVisibility(DependencyObject d, object baseValue)
        {
            if (d is not DefaultEdgePointer ecb || !ecb.IsSuppressed)
            {
                return baseValue;
            }
            return Visibility.Collapsed;
        }

        private static EdgeControl GetEdgeControl(DependencyObject parent)
        {
            while (parent != null)
            {
                if (parent is EdgeControl control)
                {
                    return control;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// When the IsSuppressed value changes, this callback triggers coercion of the Visibility property.
        /// </summary>
        private static void OnSuppressChanged(object source, DependencyPropertyChangedEventArgs args)
        {
        }

        private void EdgePointer_LayoutUpdated(object sender, DefaultEventArgs e)
        {
            if (LastKnownRectSize != Rect.Empty && !double.IsNaN(LastKnownRectSize.Width) && LastKnownRectSize.Width != 0
                && EdgeControl != null)
            {
                Arrange(LastKnownRectSize);
            }
        }

        private DependencyObject GetParent()
        {
            return Parent;
        }
    }
}