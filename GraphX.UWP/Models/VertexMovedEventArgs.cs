﻿using Windows.UI.Xaml.Input;

namespace GraphX.Controls.Models
{
    public sealed class VertexMovedEventArgs : System.EventArgs
    {
        public PointerRoutedEventArgs Args { get; private set; }
        public VertexControl VertexControl { get; private set; }

        public VertexMovedEventArgs(VertexControl vc, PointerRoutedEventArgs e)
        {
            Args = e;
            VertexControl = vc;
        }
    }
}