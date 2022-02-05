using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    public static class CustomHelper
    {
        public static FrameworkElement FindDescendantByName(this FrameworkElement element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (name.Equals(element.Name, StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                FrameworkElement result = (VisualTreeHelper.GetChild(element, i) as FrameworkElement).FindDescendantByName(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}