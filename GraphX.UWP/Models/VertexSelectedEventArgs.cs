using Windows.UI.Xaml.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexSelectedEventArgs : System.EventArgs
    {
        public PointerRoutedEventArgs Args { get; private set; }
        public VertexControl VertexControl { get; private set; }

        public VertexSelectedEventArgs(VertexControl vc, PointerRoutedEventArgs e)
        {
            VertexControl = vc;
            Args = e;
        }
    }
}