using System;

using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace GraphX.Controls
{
    public class RoundedValueConverter : IValueConverter
    {
        public int Precision { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return Math.Round((double)value, Precision);
            }
            else if (value is Point point)
            {
                return new Point(Math.Round(point.X, Precision), Math.Round(point.Y, Precision));
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}