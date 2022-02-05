using Windows.Foundation;

namespace GraphX.Controls.Models
{
    public delegate void ContentSizeChangedEventHandler(object sender, ContentSizeChangedEventArgs e);

    public sealed class ContentSizeChangedEventArgs : System.EventArgs
    {
        public Rect NewSize { get; private set; }
        public Rect OldSize { get; private set; }

        public ContentSizeChangedEventArgs(Rect oldSize, Rect newSize)
        {
            OldSize = oldSize;
            NewSize = newSize;
        }
    }
}