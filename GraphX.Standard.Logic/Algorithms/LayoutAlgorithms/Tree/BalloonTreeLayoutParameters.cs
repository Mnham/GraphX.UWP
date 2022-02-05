namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class BalloonTreeLayoutParameters : LayoutParametersBase
    {
        internal int minRadius = 2;
        internal float border = 20.0f;

        public int MinRadius
        {
            get => minRadius;
            set => SetProperty(ref minRadius, value);
        }

        public float Border
        {
            get => border;
            set => SetProperty(ref border, value);
        }
    }
}