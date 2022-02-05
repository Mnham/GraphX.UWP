using System;

using Windows.UI.Xaml.Controls;

namespace GraphX.Controls.Animations
{
    public sealed class MoveSimpleAnimation : MoveAnimationBase
    {
        public MoveSimpleAnimation(TimeSpan duration)
        {
            Duration = duration;
        }

        private int _maxCount;
        private int _counter;

        public override void Cleanup()
        {
        }

        public override void RunVertexAnimation()
        {
            _maxCount = VertexStorage.Count * 2;
            _counter = 0;
            foreach (System.Collections.Generic.KeyValuePair<IGraphControl, Measure.Point> item in VertexStorage)
            {
                Control control = item.Key as Control;
                double from = GraphAreaBase.GetX(control);
                from = double.IsNaN(from) ? 0.0 : from;

                double to = item.Value.X;

                //Here we implement workaround for WinRT stupid limitations:
                // - Can't animate custom attached props

                //First we set final coordinate that doesn't affect rendering
                // --> Already set in GraphArea computation logic <-- GraphAreaBase.SetFinalX(control, to);
                //And now we animate Canvas.Left property that affect rendering
                AnimationHelper.CreateDoubleAnimation(from, to, Duration.TotalMilliseconds, "(Canvas.Left)", control, null,
                    (s, e) =>
                    {
                        //After animation is complete we set X coordinate to FinalX stored earlier
                        //This is needed to maintain old coordinates system and avoid major changes in overall library
                        GraphAreaBase.SetX(control, GraphAreaBase.GetFinalX(control));
                        _counter++;
                        if (_counter == _maxCount)
                        {
                            OnCompleted();
                        }
                    }).Begin();

                //Repeat the same for Y coordinates
                from = GraphAreaBase.GetY(control);
                from = double.IsNaN(from) ? 0.0 : from;
                to = item.Value.Y;
                //GraphAreaBase.SetFinalY(control, to);
                AnimationHelper.CreateDoubleAnimation(from, to, Duration.TotalMilliseconds, "(Canvas.Top)", control, null,
                   (s, e) =>
                   {
                       GraphAreaBase.SetY(control, GraphAreaBase.GetFinalY(control));
                       _counter++;
                       if (_counter == _maxCount)
                       {
                           OnCompleted();
                       }
                   }).Begin();
            }
        }
    }
}