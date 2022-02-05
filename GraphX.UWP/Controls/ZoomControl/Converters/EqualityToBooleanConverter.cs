using System;

using Windows.UI.Xaml.Data;

namespace GraphX.Controls
{
    public sealed class EqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? parameter : null;
        }
    }
}