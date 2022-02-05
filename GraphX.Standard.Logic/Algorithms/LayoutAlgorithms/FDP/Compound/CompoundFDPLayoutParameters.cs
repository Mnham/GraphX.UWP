namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class CompoundFDPLayoutParameters : LayoutParametersBase
    {
        private double _idealEdgeLength = 25;
        private double _elasticConstant = 0.005;
        private double _repulsionConstant = 150;
        private double _nestingFactor = 0.2;
        private double _gravitationFactor = 8;

        private int _phase1Iterations = 50;
        private int _phase2Iterations = 70;
        private int _phase3Iterations = 30;

        private double _phase2TemperatureInitialMultiplier = 0.5;
        private double _phase3TemperatureInitialMultiplier = 0.2;

        private double _temperatureDecreasing = 0.5;
        private double _temperatureFactor = 0.95;
        private double _displacementLimitMultiplier = 0.5;
        private double _separationMultiplier = 15;

        /// <summary>
        /// Gets or sets the ideal edge length.
        /// </summary>
        public double IdealEdgeLength
        {
            get => _idealEdgeLength;
            set => SetProperty(ref _idealEdgeLength, value);
        }

        /// <summary>
        /// Gets or sets the elastic constant for the edges.
        /// </summary>
        public double ElasticConstant
        {
            get => _elasticConstant;
            set => SetProperty(ref _elasticConstant, value);
        }

        /// <summary>
        /// Gets or sets the repulsion constant for the node-node
        /// repulsion.
        /// </summary>
        public double RepulsionConstant
        {
            get => _repulsionConstant;
            set => SetProperty(ref _repulsionConstant, value);
        }

        /// <summary>
        /// Gets or sets the factor of the ideal edge length for the
        /// inter-graph edges.
        /// </summary>
        public double NestingFactor
        {
            get => _nestingFactor;
            set => SetProperty(ref _nestingFactor, value);
        }

        /// <summary>
        /// Gets or sets the factor of the gravitation.
        /// </summary>
        public double GravitationFactor
        {
            get => _gravitationFactor;
            set => SetProperty(ref _gravitationFactor, value);
        }

        public int Phase1Iterations
        {
            get => _phase1Iterations;
            set => SetProperty(ref _phase1Iterations, value);
        }

        public int Phase2Iterations
        {
            get => _phase2Iterations;
            set => SetProperty(ref _phase2Iterations, value);
        }

        public int Phase3Iterations
        {
            get => _phase3Iterations;
            set => SetProperty(ref _phase3Iterations, value);
        }

        public double Phase2TemperatureInitialMultiplier
        {
            get => _phase2TemperatureInitialMultiplier;
            set => SetProperty(ref _phase2TemperatureInitialMultiplier, value);
        }

        public double Phase3TemperatureInitialMultiplier
        {
            get => _phase3TemperatureInitialMultiplier;
            set => SetProperty(ref _phase3TemperatureInitialMultiplier, value);
        }

        public double TemperatureDecreasing
        {
            get => _temperatureDecreasing;
            set => SetProperty(ref _temperatureDecreasing, value);
        }

        public double TemperatureFactor
        {
            get => _temperatureFactor;
            set => SetProperty(ref _temperatureFactor, value);
        }

        public double DisplacementLimitMultiplier
        {
            get => _displacementLimitMultiplier;
            set => SetProperty(ref _displacementLimitMultiplier, value);
        }

        public double SeparationMultiplier
        {
            get => _separationMultiplier;
            set => SetProperty(ref _separationMultiplier, value);
        }
    }
}