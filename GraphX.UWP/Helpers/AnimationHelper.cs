using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace GraphX.Controls
{
    public static class AnimationHelper
    {
        public static Storyboard CreateDoubleAnimation(
            double? from,
            double? to,
            double duration,
            string propertyName,
            FrameworkElement target,
            FillBehavior? fillBehavior = null,
            EventHandler<object> onCompleted = null)
        {
            DoubleAnimation animation = new()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EnableDependentAnimation = true,
                //EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut }
            };

            if (fillBehavior.HasValue)
            {
                animation.FillBehavior = fillBehavior.Value;
            }

            Storyboard sb = new();
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, propertyName);
            sb.Children.Add(animation);
            if (onCompleted != null)
            {
                sb.Completed += onCompleted;
            }

            return sb;
        }
    }
}