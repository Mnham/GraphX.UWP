using GraphX.Common.Enums;
using GraphX.Common.Exceptions;
using GraphX.Common.Interfaces;
using GraphX.Controls.Models;

using System;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;

using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;

namespace GraphX.Controls
{
    /// <summary>
    /// Visual vertex control
    /// </summary>

    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "PointerLeave")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplatePart(Name = "PART_vertexLabel", Type = typeof(IVertexLabelControl))]
    [TemplatePart(Name = "PART_vcproot", Type = typeof(Panel))]
    public class VertexControl : VertexControlBase
    {
        private static readonly DependencyProperty TestXProperty = DependencyProperty.Register(
            "TestX",
            typeof(double),
            typeof(VertexControl),
            new PropertyMetadata(0, Testxchanged));

        private static readonly DependencyProperty TestYProperty = DependencyProperty.Register(
            "TestY",
            typeof(double),
            typeof(VertexControl),
            new PropertyMetadata(0, Testychanged));

        /// <summary>
        /// Gets the root element which hosts VCPs so you can add them at runtime. Requires Panel-descendant template item defined named PART_vcproot.
        /// </summary>
        public Panel VCPRoot { get; protected set; }

        /// <summary>
        /// Create vertex visual control
        /// </summary>
        /// <param name="vertexData">Vertex data object</param>
        /// <param name="bindToDataObject">Bind DataContext to the Vertex data. True by default. </param>
        public VertexControl(object vertexData, bool bindToDataObject = true)
        {
            DefaultStyleKey = typeof(VertexControl);
            if (bindToDataObject)
            {
                DataContext = vertexData;
            }

            Vertex = vertexData;

            EventOptions = new VertexEventOptions(this);
            foreach (EventType item in Enum.GetValues(typeof(EventType)).Cast<EventType>())
            {
                UpdateEventhandling(item);
            }

            IsEnabledChanged += (sender, args) => VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);

            Binding xBinding = new()
            {
                Path = new PropertyPath("(Canvas.Left)"),
                Source = this
            };
            SetBinding(TestXProperty, xBinding);
            Binding yBinding = new()
            {
                Path = new PropertyPath("(Canvas.Top)"),
                Source = this
            };
            SetBinding(TestYProperty, yBinding);
        }

        /// <summary>
        /// Cleans all potential memory-holding code
        /// </summary>
        public override void Clean()
        {
            Vertex = null;
            RootArea = null;
            HighlightBehaviour.SetIsHighlightEnabled(this, false);
            DragBehaviour.SetIsDragEnabled(this, false);
            VertexLabelControl = null;

            if (EventOptions != null)
            {
                EventOptions.Clean();
            }
        }

        public T FindDescendant<T>(string name)
        {
            return (T)(object)this.FindDescendantByName(name);
        }

        /// <summary>
        /// Gets Vertex data as specified class
        /// </summary>
        /// <typeparam name="T">Class</typeparam>
        public T GetDataVertex<T>() where T : IGraphXVertex
        {
            return (T)Vertex;
        }

        protected internal virtual void UpdateEventhandling(EventType typ)
        {
            switch (typ)
            {
                case EventType.MouseClick:
                    if (EventOptions.MouseClickEnabled)
                    {
                        PointerPressed += VertexControl_Down;
                    }
                    else
                    {
                        PointerPressed -= VertexControl_Down;
                    }

                    break;

                case EventType.MouseDoubleClick:
                    // if (EventOptions.MouseDoubleClickEnabled) Poi += VertexControl_MouseDoubleClick;
                    // else MouseDoubleClick -= VertexControl_MouseDoubleClick;
                    break;

                case EventType.MouseMove:
                    if (EventOptions.MouseMoveEnabled)
                    {
                        PointerMoved += VertexControl_MouseMove;
                    }
                    else
                    {
                        PointerMoved -= VertexControl_MouseMove;
                    }

                    break;

                case EventType.MouseEnter:
                    if (EventOptions.MouseEnterEnabled)
                    {
                        PointerEntered += VertexControl_MouseEnter;
                    }
                    else
                    {
                        PointerEntered -= VertexControl_MouseEnter;
                    }

                    break;

                case EventType.MouseLeave:
                    if (EventOptions.MouseLeaveEnabled)
                    {
                        PointerExited += VertexControl_MouseLeave;
                    }
                    else
                    {
                        PointerExited -= VertexControl_MouseLeave;
                    }

                    break;
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null)
            {
                return;
            }

            VertexLabelControl ??= FindDescendant<IVertexLabelControl>("PART_vertexLabel");

            VCPRoot ??= FindDescendant<Panel>("PART_vcproot");

            if (VertexLabelControl != null)
            {
                if (ShowLabel)
                {
                    VertexLabelControl.Show();
                }
                else
                {
                    VertexLabelControl.Hide();
                }

                UpdateLayout();
                VertexLabelControl.UpdatePosition();
            }

            VertexConnectionPointsList = this.FindDescendantsOfType<IVertexConnectionPoint>().ToList();
            if (VertexConnectionPointsList.GroupBy(x => x.Id).Count(group => @group.Count() > 1) > 0)
            {
                throw new GX_InvalidDataException("Vertex connection points in VertexControl template must have unique Id!");
            }
        }

        private static void Testxchanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IPositionChangeNotify vc)
            {
                vc.OnPositionChanged();
            }
        }

        private static void Testychanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IPositionChangeNotify vc)
            {
                vc.OnPositionChanged();
            }
        }

        private void VertexControl_Down(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexSelected(this, e, null);
            }
            //e.Handled = true;
            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void VertexControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseEnter(this, e);
            }

            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        private void VertexControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                RootArea.OnVertexMouseLeave(this, e);
            }

            VisualStateManager.GoToState(this, "PointerLeave", true);
        }

        private void VertexControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootArea != null)
            {
                RootArea.OnVertexMouseMove(this, e);
            }
        }
    }
}