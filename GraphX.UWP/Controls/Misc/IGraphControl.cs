using Windows.Foundation;
using Windows.UI.Xaml;

namespace GraphX.Controls
{
    public interface IGraphControl : IPositionChangeNotify
    {
        GraphAreaBase RootArea { get; }

        Visibility Visibility { get; set; }

        void Clean();

        Point GetPosition(bool final = false, bool round = false);

        void SetPosition(Point pt, bool alsoFinal = true);

        void SetPosition(double x, double y, bool alsoFinal = true);
    }
}