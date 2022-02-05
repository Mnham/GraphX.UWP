using System.ComponentModel;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    public class ZoomContentPresenter : ContentPresenter, INotifyPropertyChanged
    {
        private Size _contentSize;

        public Size ContentSize
        {
            get => _contentSize;
            private set
            {
                if (value == _contentSize)
                {
                    return;
                }
                _contentSize = value;
                ContentSizeChanged?.Invoke(this, _contentSize);
            }
        }

        public event ContentSizeChangedHandler ContentSizeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnRenderTransformChanged()
        {
            OnPropertyChanged(nameof(RenderTransform));
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            UIElement child = Content == null
                ? null
                : VisualTreeHelper.GetChild(this, 0) as UIElement;

            if (child == null)
            {
                return arrangeBounds;
            }

            ContentSize = child.DesiredSize;
            child.Arrange(new Rect(new Point(), child.DesiredSize));

            return arrangeBounds;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
            int max = 1000000000;
            double x = double.IsInfinity(constraint.Width) ? max : constraint.Width;
            double y = double.IsInfinity(constraint.Height) ? max : constraint.Height;
            return new Size(x, y);
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}