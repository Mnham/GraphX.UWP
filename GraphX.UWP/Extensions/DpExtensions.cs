using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GraphX.Controls
{
    public static class DependencyObjectExtensions
    {
        public static IDisposable WatchProperty(this DependencyObject target,
            string propertyPath,
            DependencyPropertyChangedEventHandler handler)
        {
            return new DependencyPropertyWatcher(target, propertyPath, handler);
        }

        private class DependencyPropertyWatcher : DependencyObject, IDisposable
        {
            private static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                "Value",
                typeof(object),
                typeof(DependencyPropertyWatcher),
                new PropertyMetadata(null, ValuePropertyChanged));

            private DependencyPropertyChangedEventHandler _handler;

            public DependencyPropertyWatcher(DependencyObject target, string propertyPath, DependencyPropertyChangedEventHandler handler)
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }

                if (propertyPath == null)
                {
                    throw new ArgumentNullException(nameof(propertyPath));
                }

                _handler = handler ?? throw new ArgumentNullException(nameof(handler));

                Binding binding = new()
                {
                    Source = target,
                    Path = new PropertyPath(propertyPath),
                    Mode = BindingMode.OneWay,
                };
                BindingOperations.SetBinding(this, ValueProperty, binding);
            }

            public void Dispose()
            {
                _handler = null;
                // There is no ClearBinding method, so set a dummy binding instead
                BindingOperations.SetBinding(this, ValueProperty, new Binding());
            }

            private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                if (d is not DependencyPropertyWatcher watcher)
                {
                    return;
                }
                watcher.OnValueChanged(e);
            }

            private void OnValueChanged(DependencyPropertyChangedEventArgs e)
            {
                _handler?.Invoke(this, e);
            }
        }
    }
}