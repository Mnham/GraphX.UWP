using System;
using System.Collections.Generic;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace GraphX.Controls
{
    public static class VisualTreeHelperEx
    {
        public static DependencyObject FindAncestorByType(DependencyObject element, Type type, bool specificTypeOnly)
        {
            if (element == null)
            {
                return null;
            }

            if (element.GetType() == type)
            {
                return element;
            }

            return FindAncestorByType(VisualTreeHelper.GetParent(element), type, specificTypeOnly);
        }

        public static T FindAncestorByType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                return default;
            }

            if (depObj is T t)
            {
                return t;
            }

            return FindAncestorByType<T>(VisualTreeHelper.GetParent(depObj));
        }

        public static UIElement FindDescendantByName(UIElement element, string name)
        {
            if (element != null && (element is FrameworkElement fe) && fe.Name == name)
            {
                return element;
            }

            UIElement foundElement = null;
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.InvalidateArrange();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                UIElement visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                foundElement = FindDescendantByName(visual, name);
                if (foundElement != null)
                {
                    break;
                }
            }

            return foundElement;
        }

        public static UIElement FindDescendantByType(UIElement element, Type type)
        {
            return FindDescendantByType(element, type, true);
        }

        public static UIElement FindDescendantByType(UIElement element, Type type, bool specificTypeOnly)
        {
            if (element == null)
            {
                return null;
            }

            if (element.GetType() == type)
            {
                return element;
            }

            UIElement foundElement = null;
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.InvalidateArrange();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                UIElement visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                foundElement = FindDescendantByType(visual, type, specificTypeOnly);
                if (foundElement != null)
                {
                    break;
                }
            }

            return foundElement;
        }

        public static T FindDescendantByType<T>(UIElement element) where T : UIElement
        {
            UIElement temp = FindDescendantByType(element, typeof(T));

            return (T)temp;
        }

        public static IEnumerable<T> FindDescendantsOfType<T>(this UIElement element) where T : class
        {
            if (element == null)
            {
                yield break;
            }

            if (element is T)
            {
                yield return element as T;
            }

            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.InvalidateArrange();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is not UIElement visual)
                {
                    continue;
                }

                foreach (T item in visual.FindDescendantsOfType<T>())
                {
                    yield return item;
                }
            }
        }

        public static UIElement FindDescendantWithPropertyValue(UIElement element,
                    DependencyProperty dp, object value)
        {
            if (element == null)
            {
                return null;
            }

            if (element.GetValue(dp).Equals(value))
            {
                return element;
            }

            UIElement foundElement = null;
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.InvalidateArrange();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                UIElement visual = VisualTreeHelper.GetChild(element, i) as UIElement;
                foundElement = FindDescendantWithPropertyValue(visual, dp, value);
                if (foundElement != null)
                {
                    break;
                }
            }

            return foundElement;
        }
    }
}