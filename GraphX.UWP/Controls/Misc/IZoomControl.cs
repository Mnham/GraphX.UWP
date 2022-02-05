using Windows.UI.Xaml;

namespace GraphX.Controls
{
    /// <summary>
    /// Common imterface for all possible zoomcontrol objects
    /// </summary>
    public interface IZoomControl
    {
        double ActualHeight { get; }
        double ActualWidth { get; }
        double Height { get; set; }
        UIElement PresenterVisual { get; }
        double Width { get; set; }
        double Zoom { get; set; }
    }
}