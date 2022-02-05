using GraphX.Common.Enums;
using GraphX.Common.Interfaces;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;

using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;

namespace GraphX.Controls
{
    public abstract class GraphAreaBase : Canvas, ITrackableContent, IGraphAreaBase
    {
        public static readonly DependencyProperty DeleteAnimationProperty = DependencyProperty.Register(
            nameof(DeleteAnimation),
            typeof(IOneWayControlAnimation),
            typeof(GraphAreaBase),
            new PropertyMetadata(null, DeleteAnimationPropertyChanged));

        public static readonly DependencyProperty ExternalSettingsProperty = DependencyProperty.Register(
            nameof(ExternalSettings),
            typeof(object),
            typeof(GraphAreaBase),
            new PropertyMetadata(null));

        public static readonly DependencyProperty FinalXProperty = DependencyProperty.RegisterAttached(
            "FinalX",
            typeof(double),
            typeof(GraphAreaBase),
            new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty FinalYProperty = DependencyProperty.RegisterAttached(
            "FinalY",
            typeof(double),
            typeof(GraphAreaBase),
            new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty LogicCoreChangeActionProperty = DependencyProperty.Register(
            nameof(LogicCoreChangeAction),
            typeof(LogicCoreChangedAction),
            typeof(GraphAreaBase),
            new PropertyMetadata(LogicCoreChangedAction.None));

        public static readonly DependencyProperty MouseOverAnimationProperty = DependencyProperty.Register(
            nameof(MouseOverAnimation),
            typeof(IBidirectionalControlAnimation),
            typeof(GraphAreaBase),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MoveAnimationProperty = DependencyProperty.Register(
            nameof(MoveAnimation),
            typeof(MoveAnimationBase),
            typeof(GraphAreaBase),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PositioningCompleteProperty = DependencyProperty.RegisterAttached(
            "PositioningComplete",
            typeof(bool),
            typeof(GraphAreaBase),
            new PropertyMetadata(true));

        public static readonly DependencyProperty XProperty = DependencyProperty.RegisterAttached(
            "X",
            typeof(double),
            typeof(GraphAreaBase),
            new PropertyMetadata(double.NaN, X_changed));

        public static readonly DependencyProperty YProperty = DependencyProperty.RegisterAttached(
            "Y",
            typeof(double),
            typeof(GraphAreaBase),
            new PropertyMetadata(double.NaN, Y_changed));

        /// <summary>
        /// Gets or Sets if GraphArea is in print mode when its size is recalculated on each Measure pass
        /// </summary>
        protected bool IsInPrintMode;

        /// <summary>
        /// Gets or sets if edge route paths must be taken into consideration while determining area size
        /// </summary>
        private const bool COUNT_ROUTE_PATHS = true;

        /// <summary>
        /// The position of the bottom right corner of the most or bottom right object if UseNativeObjectArrange == false
        /// bottom-right vertex.
        /// </summary>
        private Point _bottomRight;

        /// <summary>
        /// The position of the topLeft corner of the most top-left or top left object if UseNativeObjectArrange == false
        /// vertex.
        /// </summary>
        private Point _topLeft;

        /// <summary>
        /// Automaticaly assign unique Id (if missing) for vertex and edge data classes provided as Graph in GenerateGraph() method or by Addvertex() or AddEdge() methods
        /// </summary>
        public bool AutoAssignMissingDataId { get; set; } = true;

        /// <summary>
        /// Gets the size of the GraphArea taking into account positions of the children
        /// This is the main size pointer. Don't use DesiredSize or ActualWidth props as they are simulated.
        /// </summary>
        public Rect ContentSize => new(_topLeft, _bottomRight);

        /// <summary>
        /// Gets or sets vertex and edge controls delete animation
        /// </summary>
        public IOneWayControlAnimation DeleteAnimation
        {
            get => (IOneWayControlAnimation)GetValue(DeleteAnimationProperty);
            set => SetValue(DeleteAnimationProperty, value);
        }

        /// <summary>
        ///User-defined settings storage for using in templates and converters
        /// </summary>
        public object ExternalSettings
        {
            get => GetValue(ExternalSettingsProperty);
            set => SetValue(ExternalSettingsProperty, value);
        }

        /// <summary>
        /// Action that will take place when LogicCore property is changed. Default: None.
        /// </summary>
        public LogicCoreChangedAction LogicCoreChangeAction
        {
            get => (LogicCoreChangedAction)GetValue(LogicCoreChangeActionProperty);
            set => SetValue(LogicCoreChangeActionProperty, value);
        }

        /// <summary>
        /// Gets or sets vertex and edge controls mouse over animation
        /// </summary>
        public IBidirectionalControlAnimation MouseOverAnimation
        {
            get => (IBidirectionalControlAnimation)GetValue(MouseOverAnimationProperty);
            set => SetValue(MouseOverAnimationProperty, value);
        }

        /// <summary>
        /// Gets or sets vertex and edge controls animation
        /// </summary>
        public MoveAnimationBase MoveAnimation
        {
            get => (MoveAnimationBase)GetValue(MoveAnimationProperty);
            set => SetValue(MoveAnimationProperty, value);
        }

        /// <summary>
        /// Translation of the GraphArea object
        /// </summary>
        // public Vector Translation { get; private set; }
        /// <summary>
        /// Gets or sets additional area space for each side of GraphArea. Useful for zoom adjustments.
        /// 0 by default.
        /// </summary>
        public Size SideExpansionSize { get; set; }

        internal abstract bool EdgeCurvingEnabled { get; }

        internal abstract double EdgeCurvingTolerance { get; }

        internal abstract bool EnableParallelEdges { get; }

        // INTERNAL VARIABLES FOR CONTROLS INTEROPERABILITY
        internal abstract bool IsEdgeRoutingEnabled { get; }

        /// <summary>
        /// Fires when ContentSize property is changed
        /// </summary>
        public event ContentSizeChangedEventHandler ContentSizeChanged;

        public event EdgeSelectedEventHandler EdgeDoubleClick;

        public event EdgeSelectedEventHandler EdgeMouseEnter;

        public event EdgeSelectedEventHandler EdgeMouseLeave;

        public event EdgeSelectedEventHandler EdgeMouseMove;

        /// <summary>
        /// Fires when edge routing algorithm calculation is finished
        /// </summary>
        public event EventHandler EdgeRoutingCalculationFinished;

        /// <summary>
        /// Fires when edge is selected
        /// </summary>
        public event EdgeSelectedEventHandler EdgeSelected;

        /// <summary>
        /// Fires when graph generation operation is finished
        /// </summary>
        public event EventHandler GenerateGraphFinished;

        /// <summary>
        /// Fires when layout algorithm calculation is finished
        /// </summary>
        public event EventHandler LayoutCalculationFinished;

        /// <summary>
        /// Fires when overlap removal algorithm calculation is finished
        /// </summary>
        public event EventHandler OverlapRemovalCalculationFinished;

        /// <summary>
        /// Fires when relayout operation is finished
        /// </summary>
        public event EventHandler RelayoutFinished;

        /// <summary>
        /// Fires when vertex is double clicked
        /// </summary>
        public event VertexSelectedEventHandler VertexDoubleClick;

        /// <summary>
        /// Fires when mouse is over the vertex control
        /// </summary>
        public event VertexSelectedEventHandler VertexMouseEnter;

        /// <summary>
        /// Fires when mouse leaves vertex control
        /// </summary>
        public event VertexSelectedEventHandler VertexMouseLeave;

        /// <summary>
        /// Fires when mouse is moved over the vertex control
        /// </summary>
        public event VertexMovedEventHandler VertexMouseMove;

        /// <summary>
        /// Fires when mouse up on vertex
        /// </summary>
        public event VertexSelectedEventHandler VertexMouseUp;

        /// <summary>
        /// Fires when vertex is selected
        /// </summary>
        public event VertexSelectedEventHandler VertexSelected;

        protected GraphAreaBase()
        {
            LogicCoreChangeAction = LogicCoreChangedAction.None;
        }

        public static double GetFinalX(DependencyObject obj)
        {
            return (double)obj.GetValue(FinalXProperty);
        }

        public static double GetFinalY(DependencyObject obj)
        {
            return (double)obj.GetValue(FinalYProperty);
        }

        public static bool GetPositioningComplete(DependencyObject obj)
        {
            return (bool)obj.GetValue(PositioningCompleteProperty);
        }

        public static double GetX(DependencyObject obj)
        {
            return (double)obj.GetValue(LeftProperty);
        }

        public static double GetY(DependencyObject obj)
        {
            return (double)obj.GetValue(TopProperty);
        }

        public static void SetFinalX(DependencyObject obj, double value)
        {
            obj.SetValue(FinalXProperty, value);
        }

        public static void SetFinalY(DependencyObject obj, double value)
        {
            obj.SetValue(FinalYProperty, value);
        }

        public static void SetPositioningComplete(DependencyObject obj, bool value)
        {
            obj.SetValue(PositioningCompleteProperty, value);
        }

        public static void SetX(DependencyObject obj, double value, bool alsoSetFinal = true)
        {
            obj.SetValue(XProperty, value);
            if (alsoSetFinal)
            {
                obj.SetValue(FinalXProperty, value);
            }
        }

        public static void SetY(DependencyObject obj, double value, bool alsoSetFinal = false)
        {
            obj.SetValue(YProperty, value);
            if (alsoSetFinal)
            {
                obj.SetValue(FinalYProperty, value);
            }
        }

        /// <summary>
        /// Generates and displays edges for specified vertex
        /// </summary>
        /// <param name="vc">Vertex control</param>
        /// <param name="edgeType">Type of edges to display</param>
        /// <param name="defaultVisibility">Default edge visibility on layout</param>
        public abstract void GenerateEdgesForVertex(VertexControl vc, EdgesType edgeType, Visibility defaultVisibility = Visibility.Visible);

        /// <summary>
        /// Returns all existing VertexControls addded into the layout
        /// </summary>
        /// <returns></returns>
        public abstract VertexControl[] GetAllVertexControls();

        /// <summary>
        /// Get controls related to specified control
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="resultType">Type of resulting related controls</param>
        /// <param name="edgesType">Optional edge controls type</param>
        public abstract List<IGraphControl> GetRelatedControls(IGraphControl ctrl, GraphControlType resultType = GraphControlType.VertexAndEdge, EdgesType edgesType = EdgesType.Out);

        /// <summary>
        /// Get edge controls related to specified control
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="edgesType">Edge types to query</param>
        public abstract List<IGraphControl> GetRelatedEdgeControls(IGraphControl ctrl, EdgesType edgesType = EdgesType.All);

        /// <summary>
        /// Get vertex controls related to specified control
        /// </summary>
        /// <param name="ctrl">Original control</param>
        /// <param name="edgesType">Edge types to query for vertices</param>
        public abstract List<IGraphControl> GetRelatedVertexControls(IGraphControl ctrl, EdgesType edgesType = EdgesType.All);

        public abstract VertexControl GetVertexControlAt(Point position);

        public abstract Task RelayoutGraphAsync(bool generateAllEdges = false);

        public abstract void SetPrintMode(bool value, bool offsetControls = true, int margin = 0);

        /// <summary>
        /// Compute new edge routes for all edges of the vertex
        /// </summary>
        /// <param name="vc">Vertex visual control</param>
        /// <param name="vertexDataNeedUpdate">If vertex data inside edge routing algorthm needs to be updated</param>
        internal virtual void ComputeEdgeRoutesByVertex(VertexControl vc, bool vertexDataNeedUpdate = true)
        {
        }

        internal void OnEdgeDoubleClick(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            EdgeDoubleClick?.Invoke(this, new EdgeSelectedEventArgs(edgeControl, e));
        }

        internal void OnEdgeMouseEnter(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            EdgeMouseEnter?.Invoke(this, new EdgeSelectedEventArgs(edgeControl, e));
            MouseOverAnimation?.AnimateEdgeForward(edgeControl);
        }

        internal void OnEdgeMouseLeave(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            EdgeMouseLeave?.Invoke(this, new EdgeSelectedEventArgs(edgeControl, e));
            MouseOverAnimation?.AnimateEdgeBackward(edgeControl);
        }

        internal void OnEdgeMouseMove(EdgeControl edgeControl, MouseButtonEventArgs e, ModifierKeys keys)
        {
            EdgeMouseMove?.Invoke(this, new EdgeSelectedEventArgs(edgeControl, e));
        }

        internal virtual void OnEdgeSelected(EdgeControl ec, MouseButtonEventArgs e, ModifierKeys keys)
        {
            EdgeSelected?.Invoke(this, new EdgeSelectedEventArgs(ec, e));
        }

        internal virtual void OnVertexDoubleClick(VertexControl vc, MouseButtonEventArgs e)
        {
            VertexDoubleClick?.Invoke(this, new VertexSelectedEventArgs(vc, e));
        }

        internal virtual void OnVertexMouseEnter(VertexControl vc, MouseEventArgs e)
        {
            VertexMouseEnter?.Invoke(this, new VertexSelectedEventArgs(vc, e));
            MouseOverAnimation?.AnimateVertexForward(vc);
        }

        internal virtual void OnVertexMouseLeave(VertexControl vc, MouseEventArgs e)
        {
            VertexMouseLeave?.Invoke(this, new VertexSelectedEventArgs(vc, e));
            MouseOverAnimation?.AnimateVertexBackward(vc);
        }

        internal virtual void OnVertexMouseMove(VertexControl vc, MouseEventArgs e)
        {
            VertexMouseMove?.Invoke(this, new VertexMovedEventArgs(vc, e));
        }

        internal virtual void OnVertexMouseUp(VertexControl vc, MouseButtonEventArgs e, ModifierKeys keys)
        {
            VertexMouseUp?.Invoke(this, new VertexSelectedEventArgs(vc, e));
        }

        internal virtual void OnVertexSelected(VertexControl vc, MouseButtonEventArgs e, ModifierKeys keys)
        {
            VertexSelected?.Invoke(this, new VertexSelectedEventArgs(vc, e));
        }

        /// <summary>
        /// Arranges the size of the control.
        /// </summary>
        /// <param name="arrangeSize">The arranged size of the control.</param>
        /// <returns>The size of the control.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Point minPoint = new(double.PositiveInfinity, double.PositiveInfinity);
            Point maxPoint = new(double.NegativeInfinity, double.NegativeInfinity);

            foreach (UIElement child in Children)
            {
                double x = GetX(child);
                double y = GetY(child);

                if (double.IsNaN(x) || double.IsNaN(y))
                {
                    EdgeControl ec = child as EdgeControl;
                    //not a vertex, set the coordinates of the top-left corner
                    if (ec != null)
                    {
                        x = 0;
                        y = 0;
                    }
                    else
                    {
                        continue;
                    }

                    if (COUNT_ROUTE_PATHS && ec != null)
                    {
                        IRoutingInfo routingInfo = ec.Edge as IRoutingInfo;
                        Measure.Point[] rps = routingInfo?.RoutingPoints;
                        if (rps != null)
                        {
                            foreach (Measure.Point item in rps)
                            {
                                minPoint = new Point(Math.Min(minPoint.X, item.X), Math.Min(minPoint.Y, item.Y));
                                maxPoint = new Point(Math.Max(maxPoint.X, item.X), Math.Max(maxPoint.Y, item.Y));
                            }
                        }
                    }
                }
                else
                {
                    //get the top-left corner
                    //x -= child.DesiredSize.Width * 0.5;
                    //y -= child.DesiredSize.Height * 0.5;
                    minPoint = new Point(Math.Min(minPoint.X, x), Math.Min(minPoint.Y, y));
                    maxPoint = new Point(Math.Max(maxPoint.X, x), Math.Max(maxPoint.Y, y));
                }

                child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
            }
            return new Size(10, 10);
        }

        /// <summary>
        /// Overridden measure. It calculates a size where all of
        /// of the vertices are visible.
        /// </summary>
        /// <param name="constraint">The size constraint.</param>
        /// <returns>The calculated size.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Rect oldSize = ContentSize;
            _topLeft = new Point(double.PositiveInfinity, double.PositiveInfinity);
            _bottomRight = new Point(double.NegativeInfinity, double.NegativeInfinity);

            foreach (UIElement child in Children)
            {
                //measure the child
                child.Measure(constraint);

                //get the position of the vertex
                double left = GetFinalX(child);
                double top = GetFinalY(child);

                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (double.IsNaN(left) || double.IsNaN(top))
                {
                    if (!COUNT_ROUTE_PATHS || child is not EdgeControl ec)
                    {
                        continue;
                    }

                    if (ec.Edge is not IRoutingInfo routingInfo)
                    {
                        continue;
                    }

                    Measure.Point[] rps = routingInfo.RoutingPoints;
                    if (rps == null)
                    {
                        continue;
                    }

                    foreach (Measure.Point item in rps)
                    {
                        //get the top left corner point
                        _topLeft.X = Math.Min(_topLeft.X, item.X);
                        _topLeft.Y = Math.Min(_topLeft.Y, item.Y);

                        //calculate the bottom right corner point
                        _bottomRight.X = Math.Max(_bottomRight.X, item.X);
                        _bottomRight.Y = Math.Max(_bottomRight.Y, item.Y);
                    }
                }
                else
                {
                    //get the top left corner point
                    _topLeft.X = Math.Min(_topLeft.X, left);
                    _topLeft.Y = Math.Min(_topLeft.Y, top);

                    //calculate the bottom right corner point
                    _bottomRight.X = Math.Max(_bottomRight.X, left + child.DesiredSize.Width);
                    _bottomRight.Y = Math.Max(_bottomRight.Y, top + child.DesiredSize.Height);
                }
            }
            _topLeft.X -= SideExpansionSize.Width * .5;
            _topLeft.Y -= SideExpansionSize.Height * .5;
            _bottomRight.X += SideExpansionSize.Width * .5;
            _bottomRight.Y += SideExpansionSize.Height * .5;
            Rect newSize = ContentSize;
            if (oldSize != newSize)
            {
                OnContentSizeChanged(oldSize, newSize);
            }

            return new Size(10, 10);
        }

        protected void OnContentSizeChanged(Rect oldSize, Rect newSize)
        {
            ContentSizeChanged?.Invoke(this, new ContentSizeChangedEventArgs(oldSize, newSize));
        }

        protected virtual void OnEdgeRoutingCalculationFinished()
        {
            EdgeRoutingCalculationFinished?.Invoke(this, null);
        }

        protected virtual void OnGenerateGraphFinished()
        {
            GenerateGraphFinished?.Invoke(this, null);
        }

        protected virtual void OnLayoutCalculationFinished()
        {
            LayoutCalculationFinished?.Invoke(this, null);
        }

        protected virtual void OnOverlapRemovalCalculationFinished()
        {
            OverlapRemovalCalculationFinished?.Invoke(this, null);
        }

        protected virtual void OnRelayoutFinished()
        {
            RelayoutFinished?.Invoke(this, null);
        }

        /// <summary>
        /// Deletes vertices and edges correctly after delete animation
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="removeDataObject">Also remove data object from data graph if possible</param>
        protected abstract void RemoveAnimatedControl(IGraphControl ctrl, bool removeDataObject);

        private static void DeleteAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is IOneWayControlAnimation animation)
            {
                IOneWayControlAnimation old = animation;
                old.Completed -= GraphAreaBase_Completed;
            }
            if (e.NewValue is IOneWayControlAnimation newone)
            {
                newone.Completed += GraphAreaBase_Completed;
            }
        }

        private static void GraphAreaBase_Completed(object sender, ControlEventArgs e)
        {
            e.Control?.RootArea?.RemoveAnimatedControl(e.Control, e.RemoveDataObject);
        }

        private static void X_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(LeftProperty, e.NewValue);
        }

        private static void Y_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(TopProperty, e.NewValue);
        }
    }
}