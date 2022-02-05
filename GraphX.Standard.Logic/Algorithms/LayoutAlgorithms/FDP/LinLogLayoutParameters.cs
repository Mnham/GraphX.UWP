namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class LinLogLayoutParameters : LayoutParametersBase
    {
        internal double attractionExponent = 1.0;

        public double AttractionExponent
        {
            get => attractionExponent;
            set => SetProperty(ref attractionExponent, value);
        }

        internal double repulsiveExponent;

        public double RepulsiveExponent
        {
            get => repulsiveExponent;
            set => SetProperty(ref repulsiveExponent, value);
        }

        internal double gravitationMultiplier = 0.1;

        public double GravitationMultiplier
        {
            get => gravitationMultiplier;
            set => SetProperty(ref gravitationMultiplier, value);
        }

        internal int iterationCount = 100;

        public int IterationCount
        {
            get => iterationCount;
            set => SetProperty(ref iterationCount, value);
        }
    }
}