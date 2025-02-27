﻿using GraphX.Common.Exceptions;

using System.Linq;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using DefaultEventArgs = System.Object;

namespace GraphX.Controls
{
    /// <summary>
    /// Contains different position modes for vertices
    /// </summary>
    public enum VertexLabelPositionMode
    {
        /// <summary>
        /// Vertex label is positioned on one of the sides
        /// </summary>
        Sides,

        /// <summary>
        /// Vertex label is positioned using custom coordinates
        /// </summary>
        Coordinates
    }

    public enum VertexLabelPositionSide
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top, Right, Bottom, Left
    }

    public class VertexLabelControl : ContentControl, IVertexLabelControl
    {
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            nameof(Angle),
            typeof(double),
            typeof(VertexLabelControl),
            new PropertyMetadata(0.0, AngleChanged));

        public static readonly DependencyProperty LabelPositionModeProperty = DependencyProperty.Register(
            nameof(LabelPositionMode),
            typeof(VertexLabelPositionMode),
            typeof(VertexLabelControl),
            new PropertyMetadata(VertexLabelPositionMode.Sides));

        public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
            nameof(LabelPosition),
            typeof(Point),
            typeof(VertexLabelControl),
            new PropertyMetadata(new Point()));

        public static readonly DependencyProperty LabelPositionSideProperty = DependencyProperty.Register(
            nameof(LabelPositionSide),
            typeof(VertexLabelPositionSide),
            typeof(VertexLabelControl),
            new PropertyMetadata(VertexLabelPositionSide.BottomRight));

        internal Rect LastKnownRectSize;

        /// <summary>
        /// Gets or sets label drawing angle in degrees
        /// </summary>
        public double Angle
        {
            get => (double)GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        /// <summary>
        /// Gets or sets label position if LabelPositionMode is set to Coordinates
        /// Position is always measured from top left VERTEX corner.
        /// </summary>
        public Point LabelPosition
        {
            get => (Point)GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        /// <summary>
        /// Gets or set label positioning mode
        /// </summary>
        public VertexLabelPositionMode LabelPositionMode
        {
            get => (VertexLabelPositionMode)GetValue(LabelPositionModeProperty);
            set => SetValue(LabelPositionModeProperty, value);
        }

        /// <summary>
        /// Gets or sets label position side if LabelPositionMode is set to Sides
        /// </summary>
        public VertexLabelPositionSide LabelPositionSide
        {
            get => (VertexLabelPositionSide)GetValue(LabelPositionSideProperty);
            set => SetValue(LabelPositionSideProperty, value);
        }

        public VertexLabelControl()
        {
            DefaultStyleKey = typeof(VertexLabelControl);
            LayoutUpdated += VertexLabelControl_LayoutUpdated;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }

        public void Hide()
        {
            SetValue(VisibilityProperty, Visibility.Collapsed);
        }

        public void Show()
        {
            SetValue(VisibilityProperty, Visibility.Visible);
        }

        public virtual void UpdatePosition()
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
                Point pt;
                switch (LabelPositionSide)
                {
                    case VertexLabelPositionSide.TopRight:
                        pt = new Point(vc.DesiredSize.Width, -DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.BottomRight:
                        pt = new Point(vc.DesiredSize.Width, vc.DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.TopLeft:
                        pt = new Point(-DesiredSize.Width, -DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.BottomLeft:
                        pt = new Point(-DesiredSize.Width, vc.DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.Top:
                        pt = new Point(vc.DesiredSize.Width * .5 - DesiredSize.Width * .5, -DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.Bottom:
                        pt = new Point(vc.DesiredSize.Width * .5 - DesiredSize.Width * .5, vc.DesiredSize.Height);
                        break;

                    case VertexLabelPositionSide.Left:
                        pt = new Point(-DesiredSize.Width, vc.DesiredSize.Height * .5f - DesiredSize.Height * .5);
                        break;

                    case VertexLabelPositionSide.Right:
                        pt = new Point(vc.DesiredSize.Width, vc.DesiredSize.Height * .5f - DesiredSize.Height * .5);
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

        protected virtual DependencyObject GetParent()
        {
            return Parent;
        }

        protected virtual VertexControl GetVertexControl(DependencyObject parent)
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

        private static void AngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement ctrl)
            {
                return;
            }

            if (ctrl.RenderTransform is not TransformGroup tg)
            {
                ctrl.RenderTransform = new RotateTransform { Angle = (double)e.NewValue, CenterX = .5, CenterY = .5 };
            }
            else
            {
                Transform rt = tg.Children.FirstOrDefault(a => a is RotateTransform);
                if (rt == null)
                {
                    tg.Children.Add(new RotateTransform { Angle = (double)e.NewValue, CenterX = .5, CenterY = .5 });
                }
                else
                {
                    (rt as RotateTransform).Angle = (double)e.NewValue;
                }
            }
        }

        private void VertexLabelControl_LayoutUpdated(object sender, DefaultEventArgs e)
        {
            VertexControl vc = GetVertexControl(GetParent());
            if (vc == null || !vc.ShowLabel)
            {
                return;
            }
            UpdatePosition();
        }
    }
}