namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class KKLayoutParameters : LayoutParametersBase
    {
        private double _width = 300;

        /// <summary>
        /// Width of the bounding box.
        /// </summary>
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height = 300;

        /// <summary>
        /// Height of the bounding box.
        /// </summary>
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _maxIterations = 200;

        /// <summary>
        /// Maximum number of the iterations.
        /// </summary>
        public int MaxIterations
        {
            get => _maxIterations;
            set => SetProperty(ref _maxIterations, value);
        }

        private double _k = 1;

        public double K
        {
            get => _k;
            set => SetProperty(ref _k, value);
        }

        private bool _adjustForGravity;

        /// <summary>
        /// If true, then after the layout process, the vertices will be moved, so the barycenter will be
        /// in the center point of the bounding box.
        /// </summary>
        public bool AdjustForGravity
        {
            get => _adjustForGravity;
            set => SetProperty(ref _adjustForGravity, value);
        }

        private bool _exchangeVertices;

        public bool ExchangeVertices
        {
            get => _exchangeVertices;
            set => SetProperty(ref _exchangeVertices, value);
        }

        private double _lengthFactor = 1;

        /// <summary>
        /// Multiplier of the ideal edge length. (With this parameter the user can modify the ideal edge length).
        /// </summary>
        public double LengthFactor
        {
            get => _lengthFactor;
            set => SetProperty(ref _lengthFactor, value);
        }

        private double _disconnectedMultiplier = 0.5;

        /// <summary>
        /// Ideal distance between the disconnected points (1 is equal the ideal edge length).
        /// </summary>
        public double DisconnectedMultiplier
        {
            get => _disconnectedMultiplier;
            set => SetProperty(ref _disconnectedMultiplier, value);
        }
    }
}