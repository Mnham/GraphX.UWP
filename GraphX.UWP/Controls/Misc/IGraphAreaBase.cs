using Windows.Foundation;

namespace GraphX.Controls
{
    /// <summary>
    /// Common GraphArea interface
    /// </summary>
    public interface IGraphAreaBase
    {
        Rect ContentSize { get; }

        void SetPrintMode(bool value, bool offsetControls = true, int margin = 0);
    }
}