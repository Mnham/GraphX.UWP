using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GraphX.Controls
{
    public sealed class VisibilityToBoolConverter : IValueConverter
    {
        public bool Inverted { get; set; }
        public bool Not { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Inverted ? BoolToVisibility(value) : VisibilityToBool(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Inverted ? VisibilityToBool(value) : BoolToVisibility(value);
        }

        private object BoolToVisibility(object value)
        {
            return value is bool boolean
                ? (boolean ^ Not) ? Visibility.Visible : Visibility.Collapsed
                : throw new InvalidOperationException("SuppliedValueWasNotBool");
        }

        private object VisibilityToBool(object value)
        {
            return value is Visibility visibility
                ? (visibility == Visibility.Visible) ^ Not
                : throw new InvalidOperationException("SuppliedValueWasNotVisibility");
        }
    }
}