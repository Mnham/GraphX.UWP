using GraphX.Controls.Models;

using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace GraphX.Controls.Animations
{
    public sealed class DeleteFadeAnimation : IOneWayControlAnimation
    {
        public double Duration { get; set; }

        public DeleteFadeAnimation(double duration = .3)
        {
            Duration = duration;
        }

        private void RunAnimation(IGraphControl target, bool removeDataObject)
        {
            //create and run animation
            Storyboard story = new();
            DoubleAnimation fadeAnimation = new()
            {
                Duration = new Duration(TimeSpan.FromSeconds(Duration)),
                FillBehavior = FillBehavior.Stop,
                From = 1,
                To = 0
            };
            fadeAnimation.SetDesiredFrameRate(30);
            fadeAnimation.Completed += (sender, e) => OnCompleted(target, removeDataObject);
            story.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, target as FrameworkElement);
            Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
            story.Begin();
        }

        public void AnimateVertex(VertexControl target, bool removeDataVertex = false)
        {
            RunAnimation(target, removeDataVertex);
        }

        public void AnimateEdge(EdgeControl target, bool removeDataEdge = false)
        {
            RunAnimation(target, removeDataEdge);
        }

        public event RemoveControlEventHandler Completed;

        public void OnCompleted(IGraphControl target, bool removeDataObject)
        {
            Completed?.Invoke(this, new ControlEventArgs(target, removeDataObject));
        }
    }
}