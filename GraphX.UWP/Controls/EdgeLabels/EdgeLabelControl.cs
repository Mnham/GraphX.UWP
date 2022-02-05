using GraphX.Common.Interfaces;

using System;
using System.Diagnostics;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using DefaultEventArgs = System.Object;
using Point = Windows.Foundation.Point;
using RoutedOrCommonArgs = Windows.UI.Xaml.RoutedEventArgs;
using SysRect = Windows.Foundation.Rect;
using SysSize = Windows.Foundation.Size;

namespace GraphX.Controls
{
    public abstract class EdgeLabelControl : ContentControl, IEdgeLabelControl
    {
        public static readonly DependencyProperty AlignToEdgeProperty = DependencyProperty.Register(
            nameof(AlignToEdge),
            typeof(bool),
            typeof(EdgeLabelControl),
            new PropertyMetadata(false, AlignToEdgeChanged));

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            nameof(Angle),
            typeof(double),
            typeof(EdgeLabelControl),
            new PropertyMetadata(0.0, AngleChanged));

        public static readonly DependencyProperty DisplayForSelfLoopedEdgesProperty = DependencyProperty.Register(
            nameof(DisplayForSelfLoopedEdges),
            typeof(bool),
            typeof(EdgeLabelControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty FlipOnRotationProperty = DependencyProperty.Register(
            nameof(FlipOnRotation),
            typeof(bool),
            typeof(EdgeLabelControl),
            new PropertyMetadata(true));

        public static readonly DependencyProperty LabelHorizontalOffsetProperty = DependencyProperty.Register(
            nameof(LabelHorizontalOffset),
            typeof(double),
            typeof(EdgeLabelControl),
            new PropertyMetadata(0d));

        public static readonly DependencyProperty LabelVerticalOffsetProperty = DependencyProperty.Register(
            nameof(LabelVerticalOffset),
            typeof(double),
            typeof(EdgeLabelControl),
            new PropertyMetadata(0d));

        public static readonly DependencyProperty ShowLabelProperty = DependencyProperty.Register(
            nameof(ShowLabel),
            typeof(bool),
            typeof(EdgeLabelControl),
            new PropertyMetadata(false, ShowlabelChanged));

        internal SysRect LastKnownRectSize;

        private EdgeControl _edgeControl;

        /// <summary>
        /// Gets or sets if lables should be aligned to edges and be displayed under the same angle
        /// </summary>
        public bool AlignToEdge
        {
            get => (bool)GetValue(AlignToEdgeProperty);
            set => SetValue(AlignToEdgeProperty, value);
        }

        /// <summary>
        /// Gets or sets label drawing angle in degrees
        /// </summary>
        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        /// <summary>
        /// Gets or sets if label should be visible for self looped edge
        /// </summary>
        public bool DisplayForSelfLoopedEdges
        {
            get => (bool)GetValue(DisplayForSelfLoopedEdgesProperty);
            set => SetValue(DisplayForSelfLoopedEdgesProperty, value);
        }

        /// <summary>
        /// Gets or sets if label should flip on rotation when axis changes
        /// </summary>
        public bool FlipOnRotation
        {
            get => (bool)GetValue(FlipOnRotationProperty);
            set => SetValue(FlipOnRotationProperty, value);
        }

        /// <summary>
        /// Offset for label X axis to display it along the edge
        /// </summary>
        public double LabelHorizontalOffset
        {
            get => (double)GetValue(LabelHorizontalOffsetProperty);
            set => SetValue(LabelHorizontalOffsetProperty, value);
        }

        /// <summary>
        /// Offset for label Y axis to display it above/below the edge
        /// </summary>
        public double LabelVerticalOffset
        {
            get => (double)GetValue(LabelVerticalOffsetProperty);
            set => SetValue(LabelVerticalOffsetProperty, value);
        }

        /// <summary>
        /// Show edge label.Default value is False.
        /// </summary>
        public bool ShowLabel
        {
            get => (bool)GetValue(ShowLabelProperty);
            set => SetValue(ShowLabelProperty, value);
        }

        /// <summary>
        /// Gets or sets if label should update its position and size data on visual size change. Helps to update label correctly on template manipulations. Can be turned off for better performance.
        /// </summary>
        public bool UpdateLabelOnSizeChange { get; set; }

        /// <summary>
        /// Gets or sets if label should additionaly update its position and size data on label visibility change. Can be turned off for better performance.
        /// </summary>
        public bool UpdateLabelOnVisibilityChange { get; set; }

        protected EdgeControl EdgeControl => _edgeControl ??= GetEdgeControl(GetParent());

        public EdgeLabelControl()
        {
            DefaultStyleKey = typeof(EdgeLabelControl);
            Loaded += EdgeLabelControl_Loaded;
            RenderTransformOrigin = new Point(.5, .5);
            LayoutUpdated += EdgeLabelControl_LayoutUpdated;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            SizeChanged += EdgeLabelControl_SizeChanged;
            UpdateLabelOnSizeChange = true;
            UpdateLabelOnVisibilityChange = true;
        }

        public void Dispose()
        {
            _edgeControl = null;
        }

        /// <summary>
        /// Get label rectangular size
        /// </summary>
        public SysRect GetSize()
        {
            return LastKnownRectSize;
        }

        public void Hide()
        {
            SetValue(VisibilityProperty, Visibility.Collapsed);
        }

        /// <summary>
        /// Set label rectangular size
        /// </summary>
        public void SetSize(SysRect size)
        {
            LastKnownRectSize = size;
            // TODO: ???
            //Arrange(LastKnownRectSize);
        }

        public void Show()
        {
            if (EdgeControl.IsSelfLooped && !DisplayForSelfLoopedEdges)
            {
                return;
            }
            SetValue(VisibilityProperty, Visibility.Visible);
        }

        /// <summary>
        /// Automaticaly update edge label position
        /// </summary>
        public virtual void UpdatePosition()
        {
            if (double.IsNaN(DesiredSize.Width) || DesiredSize.Width == 0)
            {
                return;
            }

            if (EdgeControl == null)
            {
                return;
            }

            if (EdgeControl.Source == null || EdgeControl.Target == null)
            {
                Debug.WriteLine("EdgeLabelControl_LayoutUpdated() -> Got empty edgecontrol!");
                return;
            }
            //if hidden
            if (Visibility != Visibility.Visible)
            {
                return;
            }

            if (EdgeControl.IsSelfLooped)
            {
                SysSize idesiredSize = DesiredSize;
                Point pt = EdgeControl.Source.GetCenterPosition();
                SetSelfLoopedSize(pt, idesiredSize);
                Arrange(LastKnownRectSize);
                return;
            }

            Point p1 = EdgeControl.SourceConnectionPoint.GetValueOrDefault();
            Point p2 = EdgeControl.TargetConnectionPoint.GetValueOrDefault();

            double edgeLength = 0;
            if (EdgeControl.Edge is IRoutingInfo routingInfo)
            {
                Point[] routePoints = routingInfo.RoutingPoints?.ToWindows();

                if (routePoints == null || routePoints.Length == 0)
                {
                    // the edge is a single segment (p1,p2)
                    edgeLength = GetLabelDistance(MathHelper.GetDistanceBetweenPoints(p1, p2));
                }
                else
                {
                    // the edge has one or more segments
                    // compute the total length of all the segments
                    edgeLength = 0;
                    int rplen = routePoints.Length;
                    for (int i = 0; i <= rplen; ++i)
                    {
                        if (i == 0)
                        {
                            edgeLength += MathHelper.GetDistanceBetweenPoints(p1, routePoints[0]);
                        }
                        else if (i == rplen)
                        {
                            edgeLength += MathHelper.GetDistanceBetweenPoints(routePoints[rplen - 1], p2);
                        }
                        else
                        {
                            edgeLength += MathHelper.GetDistanceBetweenPoints(routePoints[i - 1], routePoints[i]);
                        }
                    }
                    // find the line segment where the half distance is located
                    edgeLength = GetLabelDistance(edgeLength);
                    Point newp1 = p1;
                    Point newp2 = p2;
                    for (int i = 0; i <= rplen; ++i)
                    {
                        double lengthOfSegment;
                        if (i == 0)
                        {
                            lengthOfSegment = MathHelper.GetDistanceBetweenPoints(newp1 = p1, newp2 = routePoints[0]);
                        }
                        else if (i == rplen)
                        {
                            lengthOfSegment = MathHelper.GetDistanceBetweenPoints(newp1 = routePoints[rplen - 1], newp2 = p2);
                        }
                        else
                        {
                            lengthOfSegment = MathHelper.GetDistanceBetweenPoints(newp1 = routePoints[i - 1], newp2 = routePoints[i]);
                        }

                        if (lengthOfSegment >= edgeLength)
                        {
                            break;
                        }

                        edgeLength -= lengthOfSegment;
                    }
                    // redefine our edge points
                    p1 = newp1;
                    p2 = newp2;
                }
            }
            // The label control should be laid out on a rectangle, in the middle of the edge
            double angleBetweenPoints = MathHelper.GetAngleBetweenPoints(p1, p2);
            SysSize desiredSize = DesiredSize;
            bool flipAxis = p1.X > p2.X; // Flip axis if source is "after" target

            edgeLength = ApplyLabelHorizontalOffset(edgeLength, LabelHorizontalOffset);

            // Calculate the center point of the edge
            Point centerPoint = new(p1.X + (edgeLength * Math.Cos(angleBetweenPoints)), p1.Y - (edgeLength * Math.Sin(angleBetweenPoints)));
            if (AlignToEdge)
            {
                // If we're aligning labels to the edges make sure add the label vertical offset
                double yEdgeOffset = LabelVerticalOffset;
                if (FlipOnRotation && flipAxis && !EdgeControl.IsParallel) // If we've flipped axis, move the offset to the other side of the edge
                {
                    yEdgeOffset = -yEdgeOffset;
                }

                // Adjust offset for rotation. Remember, the offset is perpendicular from the edge tangent.
                // Slap on 90 degrees to the angle between the points, to get the direction of the offset.
                centerPoint.Y -= yEdgeOffset * Math.Sin(angleBetweenPoints + (Math.PI / 2));
                centerPoint.X += yEdgeOffset * Math.Cos(angleBetweenPoints + (Math.PI / 2));

                // Angle is in degrees
                Angle = -angleBetweenPoints * 180 / Math.PI;
                if (flipAxis)
                {
                    Angle += 180; // Reorient the label so that it's always "pointing north"
                }
            }

            UpdateFinalPosition(centerPoint, desiredSize);

            GraphAreaBase.SetX(this, LastKnownRectSize.X, true);
            GraphAreaBase.SetY(this, LastKnownRectSize.Y, true);
            Arrange(LastKnownRectSize);
        }

        protected virtual double ApplyLabelHorizontalOffset(double edgeLength, double offset)
        {
            if (offset == 0)
            {
                return edgeLength;
            }
            edgeLength += edgeLength / 100 * offset;
            return edgeLength;
        }

        protected virtual EdgeControl GetEdgeControl(DependencyObject parent)
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

        private static void AlignToEdgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EdgeLabelControl ctrl = (EdgeLabelControl)d;
            if ((bool)e.NewValue == false)
            {
                ctrl.Angle = 0;
            }
            ctrl.UpdatePosition();
        }

        private static void AngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement ctrl)
            {
                return;
            }

            double value = (double)e.NewValue;
            if (double.IsNaN(value))
            {
                return;
            }

            if (ctrl.RenderTransform is not TransformGroup tg)
            {
                ctrl.RenderTransform = new RotateTransform { Angle = value, CenterX = .5, CenterY = .5 };
            }
            else
            {
                RotateTransform rt = (RotateTransform)tg.Children.FirstOrDefault(a => a is RotateTransform);
                if (rt == null)
                {
                    tg.Children.Add(new RotateTransform { Angle = value, CenterX = .5, CenterY = .5 });
                }
                else
                {
                    rt.Angle = value;
                }
            }
        }

        private static double GetLabelDistance(double edgeLength)
        {
            return edgeLength * .5;
        }

        private static void ShowlabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EdgeLabelControl)?.EdgeControl.UpdateEdge();
        }

        private void EdgeLabelControl_LayoutUpdated(object sender, DefaultEventArgs e)
        {
            if (EdgeControl == null || !ShowLabel)
            {
                return;
            }

            if (LastKnownRectSize == SysRect.Empty || double.IsNaN(LastKnownRectSize.Width) || LastKnownRectSize.Width == 0)
            {
                UpdateLayout();
                UpdatePosition();
            }
            else
            {
                Arrange(LastKnownRectSize);
            }
        }

        private void EdgeLabelControl_Loaded(object sender, RoutedOrCommonArgs e)
        {
            if (EdgeControl.IsSelfLooped && !DisplayForSelfLoopedEdges)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void EdgeLabelControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!UpdateLabelOnSizeChange)
            {
                return;
            }
            UpdatePosition();
        }

        private DependencyObject GetParent()
        {
            return Parent;
        }

        private void SetSelfLoopedSize(Point pt, SysSize idesiredSize)
        {
            double offsetX = -idesiredSize.Width / 2;
            double offsetY = (EdgeControl.Source.DesiredSize.Height * .5) + 2 + (idesiredSize.Height * .5);
            pt = pt.Offset(offsetX, offsetY);
            LastKnownRectSize = new SysRect(pt.X, pt.Y, idesiredSize.Width, idesiredSize.Height);
        }

        private void UpdateFinalPosition(Point centerPoint, SysSize desiredSize)
        {
            if (double.IsNaN(centerPoint.X))
            {
                centerPoint.X = 0;
            }

            if (double.IsNaN(centerPoint.Y))
            {
                centerPoint.Y = 0;
            }

            LastKnownRectSize = new SysRect(
                centerPoint.X - (desiredSize.Width / 2),
                centerPoint.Y - (desiredSize.Height / 2),
                desiredSize.Width,
                desiredSize.Height);
        }
    }
}