using GraphX.Common.Enums;
using GraphX.Common.Interfaces;
using GraphX.Controls.Models;

using System;
using System.Linq;

using Windows.UI.Xaml;

using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;

using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual edge control
    /// </summary>
    public class EdgeControl : EdgeControlBase
    {
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(EdgeControl),
            new PropertyMetadata(5.0));

        private static readonly DependencyProperty IsSelfLoopedProperty = DependencyProperty.Register(
            nameof(IsSelfLooped),
            typeof(bool),
            typeof(EdgeControl),
            new PropertyMetadata(false));

        private VertexControl _oldSource;

        private VertexControl _oldTarget;

        /// <summary>
        /// Gets if this edge is self looped (have same Source and Target)
        /// </summary>
        public override bool IsSelfLooped
        {
            get => IsSelfLoopedInternal;
            protected set => SetValue(IsSelfLoopedProperty, value);
        }

        /// <summary>
        /// Custom edge thickness
        /// </summary>
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        private bool IsSelfLoopedInternal => Source != null && Target != null && Source.Vertex == Target.Vertex;

        public EdgeControl() : this(null, null, null)
        {
        }

        public EdgeControl(VertexControl source, VertexControl target, object edge, bool showArrows = true)
        {
            DataContext = edge;
            Source = source;
            Target = target;
            Edge = edge;
            DataContext = edge;
            this.SetCurrentValue(ShowArrowsProperty, showArrows);
            IsHiddenEdgesUpdated = true;

            DefaultStyleKey = typeof(EdgeControl);

            EventOptions = new EdgeEventOptions(this);
            foreach (EventType item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
            {
                UpdateEventhandling(item);
            }

            // Trigger initial state
            SourceChanged();
            TargetChanged();

            IsSelfLooped = IsSelfLoopedInternal;
        }

        public override void Clean()
        {
            Source = null;
            Target = null;
            Edge = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            Linegeometry = null;
            LinePathObject = null;
            SelfLoopIndicator = null;

            foreach (IEdgeLabelControl edgeLabelControl in EdgeLabelControls)
            {
                edgeLabelControl.Dispose();
            }

            EdgeLabelControls.Clear();

            if (EdgePointerForSource != null)
            {
                EdgePointerForSource.Dispose();
                EdgePointerForSource = null;
            }
            if (EdgePointerForTarget != null)
            {
                EdgePointerForTarget.Dispose();
                EdgePointerForTarget = null;
            }
            EventOptions?.Clean();
        }

        public override void Dispose()
        {
            Clean();
        }

        /// <summary>
        /// Gets Edge data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataEdge<T>() where T : IGraphXCommonEdge
        {
            return (T)Edge;
        }

        protected internal virtual void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled)
                    {
                        PointerPressed += EdgeControl_MouseDown;
                    }
                    else
                    {
                        PointerPressed -= EdgeControl_MouseDown;
                    }

                    break;

                case EventType.MouseDoubleClick:
                    //if (EventOptions.MouseDoubleClickEnabled) MouseDoubleClick += EdgeControl_MouseDoubleClick;
                    //else MouseDoubleClick -= EdgeControl_MouseDoubleClick;
                    break;

                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled)
                    {
                        PointerEntered += EdgeControl_MouseEnter;
                    }
                    else
                    {
                        PointerEntered -= EdgeControl_MouseEnter;
                    }

                    break;

                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled)
                    {
                        PointerExited += EdgeControl_MouseLeave;
                    }
                    else
                    {
                        PointerExited -= EdgeControl_MouseLeave;
                    }

                    break;

                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled)
                    {
                        PointerMoved += EdgeControl_MouseMove;
                    }
                    else
                    {
                        PointerMoved -= EdgeControl_MouseMove;
                    }

                    break;
            }
        }

        protected override void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SourceChanged();
        }

        protected override void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TargetChanged();
        }

        private void EdgeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnEdgeDoubleClick(this, e, Keyboard.Modifiers);
            }
            //e.Handled = true;
        }

        private void EdgeControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnEdgeSelected(this, e, Keyboard.Modifiers);
            }

            e.Handled = true;
        }

        private void EdgeControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnEdgeMouseEnter(this, null, Keyboard.Modifiers);
            }
            // e.Handled = true;
        }

        private void EdgeControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnEdgeMouseLeave(this, null, Keyboard.Modifiers);
            }
            // e.Handled = true;
        }

        private void EdgeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnEdgeMouseMove(this, null, Keyboard.Modifiers);
            }
            // e.Handled = true;
        }

        private void Source_PositionChanged(object sender, EventArgs e)
        {
            //update edge on any connected vertex position changes
            UpdateEdge();
        }

        private void Source_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateEdge();
        }

        private void SourceChanged()
        {
            // Only proceed if not in design mode
            if (EventOptions == null)
            {
                return;
            }

            if (_oldSource != null)
            {
                _oldSource.PositionChanged -= Source_PositionChanged;
                _oldSource.SizeChanged -= Source_SizeChanged;
            }
            _oldSource = Source;
            if (Source != null)
            {
                Source.PositionChanged += Source_PositionChanged;
                Source.SizeChanged += Source_SizeChanged;
            }
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }

        private void TargetChanged()
        {
            // Only proceed if not in design mode
            if (EventOptions == null)
            {
                return;
            }

            if (_oldTarget != null)
            {
                _oldTarget.PositionChanged -= Source_PositionChanged;
                _oldTarget.SizeChanged -= Source_SizeChanged;
            }
            _oldTarget = Target;
            if (Target != null)
            {
                Target.PositionChanged += Source_PositionChanged;
                Target.SizeChanged += Source_SizeChanged;
            }
            IsSelfLooped = IsSelfLoopedInternal;
            UpdateSelfLoopedEdgeData();
        }
    }
}