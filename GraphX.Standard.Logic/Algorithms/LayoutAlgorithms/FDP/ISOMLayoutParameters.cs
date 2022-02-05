namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class ISOMLayoutParameters : LayoutParametersBase
    {
        private double _width = 300;

        /// <summary>
        /// Width of the bounding box. Default value is 300.
        /// </summary>
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height = 300;

        /// <summary>
        /// Height of the bounding box. Default value is 300.
        /// </summary>
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _maxEpoch = 2000;

        /// <summary>
        /// Maximum iteration number. Default value is 2000.
        /// </summary>
        public int MaxEpoch
        {
            get => _maxEpoch;
            set => SetProperty(ref _maxEpoch, value);
        }

        private int _radiusConstantTime = 100;

        /// <summary>
        /// Radius constant time. Default value is 100.
        /// </summary>
        public int RadiusConstantTime
        {
            get => _radiusConstantTime;
            set => SetProperty(ref _radiusConstantTime, value);
        }

        private int _initialRadius = 5;

        /// <summary>
        /// Default value is 5.
        /// </summary>
        public int InitialRadius
        {
            get => _initialRadius;
            set => SetProperty(ref _initialRadius, value);
        }

        private int _minRadius = 1;

        /// <summary>
        /// Minimal radius. Default value is 1.
        /// </summary>
        public int MinRadius
        {
            get => _minRadius;
            set => SetProperty(ref _minRadius, value);
        }

        private double _initialAdaption = 0.9;

        /// <summary>
        /// Default value is 0.9.
        /// </summary>
        public double InitialAdaption
        {
            get => _initialAdaption;
            set => SetProperty(ref _initialAdaption, value);
        }

        private double _minAdaption;

        /// <summary>
        /// Default value is 0.
        /// </summary>
        public double MinAdaption
        {
            get => _minAdaption;
            set => SetProperty(ref _minAdaption, value);
        }

        private double _coolingFactor = 2;

        /// <summary>
        /// Default value is 2.
        /// </summary>
        public double CoolingFactor
        {
            get => _coolingFactor;
            set => SetProperty(ref _coolingFactor, value);
        }
    }
}