using GraphX.Controls.Models;
using GraphX.Measure;

using Microsoft.Toolkit.Mvvm.Input;

using System;
using System.ComponentModel;

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;
using Thickness = Windows.UI.Xaml.Thickness;

namespace GraphX.Controls
{
    [TemplatePart(Name = PART_PRESENTER, Type = typeof(ZoomContentPresenter))]
    public class ZoomControl : ContentControl, IZoomControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty AnimationLengthProperty = DependencyProperty.Register(
            nameof(AnimationLength),
            typeof(TimeSpan),
            typeof(ZoomControl),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

        public static readonly DependencyProperty MaximumZoomStepProperty = DependencyProperty.Register(
            nameof(MaximumZoomStep),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(5.0));

        public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
            nameof(MaxZoom),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(5.0));

        public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
            nameof(MinZoom),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(0.1));

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            nameof(Mode),
            typeof(ZoomControlModes),
            typeof(ZoomControl),
            new PropertyMetadata(ZoomControlModes.Custom, Mode_PropertyChanged));

        public static readonly DependencyProperty ModifierModeProperty = DependencyProperty.Register(
            nameof(ModifierMode),
            typeof(ZoomViewModifierMode),
            typeof(ZoomControl),
            new PropertyMetadata(ZoomViewModifierMode.None));

        public static readonly DependencyProperty TranslateXProperty = DependencyProperty.Register(
            nameof(TranslateX),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(0.0, TranslateX_PropertyChanged));

        public static readonly DependencyProperty TranslateYProperty = DependencyProperty.Register(
            nameof(TranslateY),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(0.0, TranslateY_PropertyChanged));

        public static readonly DependencyProperty ZoomBoxBackgroundProperty = DependencyProperty.Register(
            nameof(ZoomBoxBackground),
            typeof(Brush),
            typeof(ZoomControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomBoxBorderBrushProperty = DependencyProperty.Register(
            nameof(ZoomBoxBorderBrush),
            typeof(Brush),
            typeof(ZoomControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomBoxBorderThicknessProperty = DependencyProperty.Register(
            nameof(ZoomBoxBorderThickness),
            typeof(Thickness),
            typeof(ZoomControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomBoxOpacityProperty = DependencyProperty.Register(
            nameof(ZoomBoxOpacity),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(0.5));

        public static readonly DependencyProperty ZoomBoxProperty = DependencyProperty.Register(
            nameof(ZoomBox),
            typeof(Rect),
            typeof(ZoomControl),
            new PropertyMetadata(new Rect()));

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            nameof(Zoom),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(1.0, Zoom_PropertyChanged));

        public static readonly DependencyProperty ZoomSensitivityProperty = DependencyProperty.Register(
            nameof(ZoomSensitivity),
            typeof(double),
            typeof(ZoomControl),
            new PropertyMetadata(80.0));

        private const string PART_PRESENTER = "PART_Presenter";
        private Storyboard _currentZoomAnimation;
        private bool _isZooming;
        private Storyboard _lastTranslateXAnimation;
        private Storyboard _lastTranslateYAnimation;
        private Point _mouseDownPos;
        private ZoomContentPresenter _presenter;
        private ScaleTransform _scaleTransform;
        private bool _startedAsAreaSelection;
        private Vector _startTranslate;
        private TransformGroup _transformGroup;
        private TranslateTransform _translateTransform;
        private int _zoomAnimCount;

        public TimeSpan AnimationLength
        {
            get => (TimeSpan)GetValue(AnimationLengthProperty);
            set => SetValue(AnimationLengthProperty, value);
        }

        public RelayCommand CenterToContentCommand => new(CenterContent);

        /// <summary>
        /// Gets content object as UIElement
        /// </summary>
        public UIElement ContentVisual => Content as UIElement;

        /// <summary>
        /// Gets or sets if animation should be disabled
        /// </summary>
        public bool IsAnimationDisabled { get; set; }

        /// <summary>
        /// Is loaded content represents ITrackableContent object
        /// </summary>
        public bool IsContentTrackable { get; private set; }

        public double MaximumZoomStep
        {
            get => (double)GetValue(MaximumZoomStepProperty);
            set => SetValue(MaximumZoomStepProperty, value);
        }

        public double MaxZoom
        {
            get => (double)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }

        public double MinZoom
        {
            get => (double)GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        /// <summary>
        /// Gets or sets the mode of the zoom control.
        /// </summary>
        public ZoomControlModes Mode
        {
            get => (ZoomControlModes)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the active modifier mode.
        /// </summary>
        public ZoomViewModifierMode ModifierMode
        {
            get => (ZoomViewModifierMode)GetValue(ModifierModeProperty);
            set => SetValue(ModifierModeProperty, value);
        }

        /// <summary>
        /// Gets or sets absolute zooming on mouse wheel which doesn't depend on mouse position
        /// </summary>
        public MouseWheelZoomingMode MouseWheelZoomingMode { get; set; }

        public Point OrigoPosition => new(ActualWidth / 2, ActualHeight / 2);

        public ZoomContentPresenter Presenter
        {
            get => _presenter;
            set
            {
                _presenter = value;
                if (_presenter == null)
                {
                    return;
                }

                //add the ScaleTransform to the presenter
                _transformGroup = new TransformGroup();
                _scaleTransform = new ScaleTransform();
                _translateTransform = new TranslateTransform();
                _transformGroup.Children.Add(_scaleTransform);
                _transformGroup.Children.Add(_translateTransform);
                _presenter.RenderTransform = _transformGroup;
                _presenter.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        public UIElement PresenterVisual => Presenter;

        /// <summary>
        /// Gets content as ITrackableContent like GraphArea
        /// </summary>
        public ITrackableContent TrackableContent => Content as ITrackableContent;

        public double TranslateX
        {
            get
            {
                double value = (double)GetValue(TranslateXProperty);
                return double.IsNaN(value) ? 0 : value;
            }
            set
            {
                if (_lastTranslateXAnimation != null)
                {
                    //_lastTranslateXAnimation.SkipToFill();
                    _lastTranslateXAnimation.Stop();
                    //SetValue(TranslateXProperty, TranslateX);
                }
                _lastTranslateXAnimation = AnimationHelper.CreateDoubleAnimation(TranslateX, value, 0, nameof(TranslateX), this, null, (o, e) => SetValue(TranslateXProperty, value));
                // ((DoubleAnimation)_lastTranslateXAnimation.Children[0]).EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
                _lastTranslateXAnimation.Begin();
                //SetValue(TranslateXProperty, value);
            }
        }

        public double TranslateY
        {
            get
            {
                double value = (double)GetValue(TranslateYProperty);
                return double.IsNaN(value) ? 0 : value;
            }
            set
            {
                if (_lastTranslateYAnimation != null)
                {
                    //_lastTranslateYAnimation.SkipToFill();
                    _lastTranslateYAnimation.Stop();
                    //SetValue(TranslateYProperty, TranslateY);
                }

                _lastTranslateYAnimation = AnimationHelper.CreateDoubleAnimation(TranslateY, value, 0, nameof(TranslateY), this, null, (o, e) => SetValue(TranslateYProperty, value));
                //((DoubleAnimation)_lastTranslateYAnimation.Children[0]).EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
                _lastTranslateYAnimation.Begin();
                //SetValue(TranslateYProperty, value);
            }
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set
            {
                if (value == (double)GetValue(ZoomProperty))
                {
                    return;
                }
                //TODO BeginAnimation(ZoomProperty, null);
                SetValue(ZoomProperty, value);
            }
        }

        public Rect ZoomBox
        {
            get => (Rect)GetValue(ZoomBoxProperty);
            set => SetValue(ZoomBoxProperty, value);
        }

        public Brush ZoomBoxBackground
        {
            get => (Brush)GetValue(ZoomBoxBackgroundProperty);
            set => SetValue(ZoomBoxBackgroundProperty, value);
        }

        public Brush ZoomBoxBorderBrush
        {
            get => (Brush)GetValue(ZoomBoxBorderBrushProperty);
            set => SetValue(ZoomBoxBorderBrushProperty, value);
        }

        public Thickness ZoomBoxBorderThickness
        {
            get => (Thickness)GetValue(ZoomBoxBorderThicknessProperty);
            set => SetValue(ZoomBoxBorderThicknessProperty, value);
        }

        public double ZoomBoxOpacity
        {
            get => (double)GetValue(ZoomBoxOpacityProperty);
            set => SetValue(ZoomBoxOpacityProperty, value);
        }

        public double ZoomSensitivity
        {
            get => (double)GetValue(ZoomSensitivityProperty);
            set => SetValue(ZoomSensitivityProperty, value);
        }

        public RelayCommand ZoomToFillCommand => new(ZoomToFill);

        /// <summary>
        /// Fires when area has been selected using SelectionModifiers
        /// </summary>
        public event AreaSelectedEventHandler AreaSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler ZoomAnimationCompleted;

        public ZoomControl()
        {
            DefaultStyleKey = typeof(ZoomControl);
            PointerWheelChanged += ZoomControl_MouseWheel;
            PointerPressed += ZoomControl_PreviewMouseDown;
            PointerReleased += ZoomControl_MouseUp;
            Loaded += ZoomControl_Loaded;
        }

        /// <summary>
        /// Centers content on the screen
        /// </summary>
        public void CenterContent()
        {
            if (_presenter == null)
            {
                return;
            }

            Vector initialTranslate = GetTrackableTranslate();
            DoZoomAnimation(Zoom, initialTranslate.X * Zoom, initialTranslate.Y * Zoom);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Converts screen rectangle area to rectangle in content coordinate space according to scale and translation
        /// </summary>
        /// <param name="screenRectangle">Screen rectangle data</param>
        public Rect ToContentRectangle(Rect screenRectangle)
        {
            GeneralTransform transformer = TransformToVisual(ContentVisual);
            Point tl = transformer.TransformPoint(new Point(screenRectangle.X, screenRectangle.Y));
            Point br = transformer.TransformPoint(new Point(screenRectangle.Right, screenRectangle.Bottom));
            return new Rect(tl.X, tl.Y, Math.Abs(Math.Abs(br.X) - Math.Abs(tl.X)), Math.Abs(Math.Abs(br.Y) - Math.Abs(tl.Y)));
        }

        /// <summary>
        /// Zoom to rectangle area of the content
        /// </summary>
        /// <param name="rectangle">Rectangle area</param>
        /// <param name="usingContentCoordinates">Sets if content coordinates or screen coordinates was specified</param>
        public void ZoomToContent(Rect rectangle, bool usingContentCoordinates = true)
        {
            //if content isn't UIElement - return
            if (ContentVisual == null)
            {
                return;
            }
            // translate the region from the coordinate space of the content
            // to the coordinate space of the content presenter
            GeneralTransform transformer = ContentVisual.TransformToVisual(_presenter);
            Rect region = usingContentCoordinates ?
              new Rect(
                transformer.TransformPoint(new Point(rectangle.Top, rectangle.Left)),
                transformer.TransformPoint(new Point(rectangle.Bottom, rectangle.Right))) : rectangle;

            // calculate actual zoom, which must fit the entire selection
            // while maintaining a 1:1 ratio
            double aspectX = ActualWidth / region.Width;
            double aspectY = ActualHeight / region.Height;
            double newRelativeScale = aspectX < aspectY ? aspectX : aspectY;
            // ensure that the scale value alls within the valid range
            if (newRelativeScale > MaxZoom)
            {
                newRelativeScale = MaxZoom;
            }
            else if (newRelativeScale < MinZoom)
            {
                newRelativeScale = MinZoom;
            }

            Point center = new(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2));
            Point newRelativePosition = new(((ActualWidth / 2) - center.X) * Zoom, ((ActualHeight / 2) - center.Y) * Zoom);

            TranslateX = newRelativePosition.X;
            TranslateY = newRelativePosition.Y;
            Zoom = newRelativeScale;
        }

        /// <summary>
        /// Zoom to fill screen area with the content
        /// </summary>
        public void ZoomToFill()
        {
            if (Mode == ZoomControlModes.Fill)
            {
                DoZoomToFill();
            }
            else
            {
                Mode = ZoomControlModes.Fill;
            }
        }

        /// <summary>
        /// Zoom to original size
        /// </summary>
        public void ZoomToOriginal()
        {
            if (Mode == ZoomControlModes.Original)
            {
                DoZoomToOriginal();
            }
            else
            {
                Mode = ZoomControlModes.Original;
            }
        }

        /// <summary>
        /// Defines action on mousewheel
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="mousePosition"></param>
        protected virtual void MouseWheelAction(int delta, Point mousePosition)
        {
            Point origoPosition = OrigoPosition;

            double deltaZoom = (Math.Abs(delta) / 10000.0 * ZoomSensitivity) + 1;
            deltaZoom = Math.Min(MaximumZoomStep, deltaZoom);
            deltaZoom = Math.Max(1.0 / MaximumZoomStep, deltaZoom);

            DoZoom(
                 deltaZoom,
                 delta < 0 ? -1 : 1,
                 origoPosition,
                 MouseWheelZoomingMode == MouseWheelZoomingMode.Absolute ? origoPosition : mousePosition,
                 MouseWheelZoomingMode == MouseWheelZoomingMode.Absolute ? origoPosition : mousePosition);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //VF AttachToVisualTree();

            //get the presenter, and initialize
            Presenter = GetTemplateChild(PART_PRESENTER) as ZoomContentPresenter;
            if (Presenter != null)
            {
                Presenter.SizeChanged += (s, a) =>
                {
                    //VF UpdateViewport();
                    if (Mode == ZoomControlModes.Fill)
                    {
                        DoZoomToFill();
                    }
                };
                Presenter.ContentSizeChanged += (s, a) =>
                {
                    //UpdateViewFinderDisplayContentBounds();
                    if (Mode == ZoomControlModes.Fill)
                    {
                        DoZoomToFill();
                        //IsAnimationDisabled = false;
                    }
                };
            }
            ZoomToFill();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (oldContent != null)
            {
                if (oldContent is ITrackableContent old)
                {
                    old.ContentSizeChanged -= Content_ContentSizeChanged;
                }
            }
            if (newContent != null)
            {
                //VF UpdateViewFinderDisplayContentBounds();
                //VF UpdateViewport();
                if (newContent is ITrackableContent newc)
                {
                    IsContentTrackable = true;
                    newc.ContentSizeChanged += Content_ContentSizeChanged;
                }
                else
                {
                    IsContentTrackable = false;
                }
            }

            base.OnContentChanged(oldContent, newContent);
        }

        private static void Mode_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomControl zc = (ZoomControl)d;
            ZoomControlModes mode = (ZoomControlModes)e.NewValue;
            switch (mode)
            {
                case ZoomControlModes.Fill:
                    zc.DoZoomToFill();
                    break;

                case ZoomControlModes.Original:
                    zc.DoZoomToOriginal();
                    break;

                case ZoomControlModes.Custom:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void TranslateX_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomControl zc = (ZoomControl)d;
            if (zc._translateTransform == null)
            {
                return;
            }

            zc._translateTransform.X = (double)e.NewValue;
            if (!zc._isZooming)
            {
                zc.Mode = ZoomControlModes.Custom;
            }

            zc.OnPropertyChanged(nameof(Presenter));
            zc.Presenter.OnRenderTransformChanged();
        }

        private static void TranslateY_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomControl zc = (ZoomControl)d;
            if (zc._translateTransform == null)
            {
                return;
            }

            zc._translateTransform.Y = (double)e.NewValue;
            if (!zc._isZooming)
            {
                zc.Mode = ZoomControlModes.Custom;
            }

            zc.OnPropertyChanged(nameof(Presenter));
            zc.Presenter.OnRenderTransformChanged();
        }

        private static void Zoom_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomControl zc = (ZoomControl)d;

            if (zc._scaleTransform == null)
            {
                return;
            }

            double zoom = (double)e.NewValue;
            zc._scaleTransform.ScaleX = zoom;
            zc._scaleTransform.ScaleY = zoom;
            /* if (!zc._isZooming)
             {
                 var delta = (double)e.NewValue / (double)e.OldValue;
                 zc.TranslateX *= delta;
                 zc.TranslateY *= delta;
                 zc.Mode = ZoomControlModes.Custom;
             }*/
            zc.OnPropertyChanged(nameof(Presenter));
            zc.Presenter.OnRenderTransformChanged();
            zc.OnPropertyChanged(nameof(Zoom));

            //VF zc.UpdateViewport();
        }

        private void Content_ContentSizeChanged(object sender, ContentSizeChangedEventArgs e)
        {
            //VF UpdateViewFinderDisplayContentBounds();
            //VF UpdateViewport();
        }

        private void DoZoom(double deltaZoom, int mod, Point origoPosition, Point startHandlePosition, Point targetHandlePosition, bool setDelta = false)
        {
            double startZoom = Zoom;
            double currentZoom = setDelta
                ? deltaZoom
                : (mod == -1
                    ? (startZoom / deltaZoom)
                    : (startZoom * deltaZoom));

            currentZoom = Math.Max(MinZoom, Math.Min(MaxZoom, currentZoom));

            Point startTranslate = new(TranslateX, TranslateY);

            Point v = startHandlePosition.Subtract(origoPosition);
            Point vTarget = targetHandlePosition.Subtract(origoPosition);

            Point targetPoint = v.Subtract(startTranslate).Div(startZoom);
            Point zoomedTargetPointPos = targetPoint.Mul(currentZoom).Sum(startTranslate);
            Point endTranslate = vTarget.Subtract(zoomedTargetPointPos);

            if (setDelta)
            {
                double transformX = GetCoercedTranslate(endTranslate.X);
                double transformY = GetCoercedTranslate(endTranslate.Y);
                DoZoomAnimation(currentZoom, transformX, transformY);
            }
            else
            {
                double transformX = GetCoercedTranslate(TranslateX + endTranslate.X);
                double transformY = GetCoercedTranslate(TranslateY + endTranslate.Y);
                DoZoomAnimation(currentZoom, transformX, transformY);
            }
            Mode = ZoomControlModes.Custom;
        }

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY, bool isZooming = true)
        {
            if (targetZoom == 0d && double.IsNaN(transformX) && double.IsNaN(transformY))
            {
                return;
            }

            _isZooming = isZooming;
            Duration duration = !IsAnimationDisabled
                ? new Duration(AnimationLength)
                : new Duration(new TimeSpan(0, 0, 0, 0, 100));

            double value = (double)GetValue(TranslateXProperty);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                SetValue(TranslateXProperty, 0d);
            }

            value = (double)GetValue(TranslateYProperty);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                SetValue(TranslateYProperty, 0d);
            }

            StartAnimation(TranslateXProperty, nameof(TranslateX), transformX, duration);
            if (double.IsNaN(transformY) || double.IsInfinity(transformY))
            {
                transformY = 0;
            }

            StartAnimation(TranslateYProperty, nameof(TranslateY), transformY, duration);
            if (double.IsNaN(targetZoom) || double.IsInfinity(targetZoom))
            {
                targetZoom = 1;
            }

            StartAnimation(ZoomProperty, nameof(Zoom), targetZoom, duration);
        }

        private void DoZoomToFill()
        {
            if (_presenter == null)
            {
                return;
            }

            Windows.Foundation.Size c = IsContentTrackable
                ? TrackableContent.ContentSize.Size()
                : ContentVisual.DesiredSize;

            double deltaZoom = Math.Min(MaxZoom, Math.Min(ActualWidth / c.Width, ActualHeight / c.Height));

            Vector initialTranslate = IsContentTrackable
                ? GetTrackableTranslate()
                : GetInitialTranslate(c.Width, c.Height);

            DoZoomAnimation(deltaZoom, initialTranslate.X * deltaZoom, initialTranslate.Y * deltaZoom);
        }

        private void DoZoomToOriginal()
        {
            if (_presenter == null)
            {
                return;
            }

            Vector initialTranslate = GetTrackableTranslate();
            DoZoomAnimation(1.0, initialTranslate.X, initialTranslate.Y);
        }

        private double GetCoercedTranslate(double baseValue)
        {
            return _presenter == null ? 0.0 : baseValue;
        }

        private Vector GetInitialTranslate(double contentWidth, double contentHeight, double offsetX = 0, double offsetY = 0)
        {
            if (_presenter == null)
            {
                return new Vector(0.0, 0.0);
            }

            double w = contentWidth - ActualWidth;
            double h = contentHeight - ActualHeight;
            double tX = -((w / 2.0) + offsetX);
            double tY = -((h / 2.0) + offsetY);

            return new Vector(tX, tY);
        }

        /// <summary>
        /// Returns initial translate depending on container graph settings (to deal with optinal new coord system)
        /// </summary>
        private Vector GetTrackableTranslate()
        {
            return IsContentTrackable
                ? GetInitialTranslate(
                TrackableContent.ContentSize.Width, TrackableContent.ContentSize.Height,
                TrackableContent.ContentSize.X, TrackableContent.ContentSize.Y)
                : new Vector();
        }

        private void OnAreaSelected(Rect selection)
        {
            AreaSelected?.Invoke(this, new AreaSelectedEventArgs(selection));
        }

        private void OnMouseDown(PointerRoutedEventArgs e, bool isPreview)
        {
            if (ModifierMode != ZoomViewModifierMode.None)
            {
                return;
            }

            _startedAsAreaSelection = false;
            switch (e.KeyModifiers)
            {
                case VirtualKeyModifiers.None:
                    if (!isPreview)
                    {
                        ModifierMode = ZoomViewModifierMode.Pan;
                    }

                    break;

                case VirtualKeyModifiers.Windows | VirtualKeyModifiers.Control:
                    _startedAsAreaSelection = true;
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;

                case VirtualKeyModifiers.Windows:
                    ModifierMode = ZoomViewModifierMode.ZoomBox;
                    break;

                case VirtualKeyModifiers.Control:
                    break;

                case VirtualKeyModifiers.Shift:
                    ModifierMode = ZoomViewModifierMode.Pan;
                    break;

                default:
                    return;
            }

            if (ModifierMode == ZoomViewModifierMode.None)
            {
                return;
            }

            _mouseDownPos = e.GetCurrentPoint(this).Position;
            _startTranslate = new Vector(TranslateX, TranslateY);
            CapturePointer(e.Pointer);
            PointerMoved += ZoomControl_PreviewMouseMove;
        }

        private void OnZoomAnimationCompleted()
        {
            ZoomAnimationCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void StartAnimation(DependencyProperty dp, string dpName, double toValue, Duration duration)
        {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue))
            {
                if (dp == ZoomProperty)
                {
                    _isZooming = false;
                }
                return;
            }

            _currentZoomAnimation = AnimationHelper.CreateDoubleAnimation(null, toValue, duration.TimeSpan.TotalMilliseconds, dpName, this);
            if (dp == ZoomProperty)
            {
                _zoomAnimCount++;
                _currentZoomAnimation.Completed += (s, args) =>
                {
                    _zoomAnimCount--;
                    if (_zoomAnimCount > 0 && _currentZoomAnimation != s)
                    {
                        return;
                    }

                    SetValue(ZoomProperty, Zoom);
                    _isZooming = false;
                    //VF UpdateViewport();
                    OnZoomAnimationCompleted();
                };
            }
            _currentZoomAnimation.Begin();
        }

        private void ZoomControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetValue(ZoomProperty, Zoom);
        }

        private void ZoomControl_MouseUp(object sender, PointerRoutedEventArgs e)
        {
            switch (ModifierMode)
            {
                case ZoomViewModifierMode.None:
                    return;

                case ZoomViewModifierMode.Pan:
                    break;

                case ZoomViewModifierMode.ZoomIn:
                    break;

                case ZoomViewModifierMode.ZoomOut:
                    break;

                case ZoomViewModifierMode.ZoomBox:
                    if (_startedAsAreaSelection)
                    {
                        _startedAsAreaSelection = false;

                        OnAreaSelected(ToContentRectangle(ZoomBox));
                        ZoomBox = Rect.Empty;
                    }
                    else
                    {
                        ZoomToInternal(ZoomBox);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            ModifierMode = ZoomViewModifierMode.None;
            PointerMoved -= ZoomControl_PreviewMouseMove;
            ReleasePointerCapture(e.Pointer);
        }

        private void ZoomControl_MouseWheel(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            int mouseWheelDelta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            Point mousePosition = e.GetCurrentPoint(this).Position;

            MouseWheelAction(mouseWheelDelta, mousePosition);
        }

        private void ZoomControl_PreviewMouseDown(object sender, PointerRoutedEventArgs e)
        {
            OnMouseDown(e, false);
            e.Handled = false;
        }

        private void ZoomControl_PreviewMouseMove(object sender, PointerRoutedEventArgs e)
        {
            Point pos = e.GetCurrentPoint(this).Position;
            switch (ModifierMode)
            {
                case ZoomViewModifierMode.None:
                    return;

                case ZoomViewModifierMode.Pan:
                    Point pps = pos.Subtract(_mouseDownPos);
                    double translatex = _startTranslate.X + pps.X;
                    double translatey = _startTranslate.Y + pps.Y;
                    TranslateX = translatex;
                    TranslateY = translatey;
                    //VF UpdateViewport();
                    break;

                case ZoomViewModifierMode.ZoomIn:
                    break;

                case ZoomViewModifierMode.ZoomOut:
                    break;

                case ZoomViewModifierMode.ZoomBox:
                    double x = Math.Min(_mouseDownPos.X, pos.X);
                    double y = Math.Min(_mouseDownPos.Y, pos.Y);
                    double sizeX = Math.Abs(_mouseDownPos.X - pos.X);
                    double sizeY = Math.Abs(_mouseDownPos.Y - pos.Y);
                    ZoomBox = new Rect(x, y, sizeX, sizeY);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ZoomToInternal(Rect rect, bool setDelta = false)
        {
            double deltaZoom = Math.Min(ActualWidth / rect.Width, ActualHeight / rect.Height);
            Point origoPosition = OrigoPosition;
            Point startHandlePosition = new(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
            DoZoom(deltaZoom, 1, origoPosition, startHandlePosition, origoPosition, setDelta);
            ZoomBox = new Rect();
        }
    }
}