using System;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    /// <summary>
    /// Parameters base for the Fruchterman-Reingold Algorithm (FDP).
    /// </summary>
    public abstract class FRLayoutParametersBase : LayoutParametersBase
    {
        #region Properties, Parameters

        private int _vertexCount;
        internal double _attractionMultiplier = 1.2;
        internal double _repulsiveMultiplier = 0.6;
        internal int _iterationLimit = 200;
        internal double _lambda = 0.95;
        internal FRCoolingFunction _coolingFunction = FRCoolingFunction.Exponential;

        /// <summary>
		/// Count of the vertices (used to calculate the constants)
		/// </summary>
		internal int VertexCount
        {
            get => _vertexCount;
            set
            {
                if (SetProperty(ref _vertexCount, value))
                {
                    UpdateParameters();
                }
            }
        }

        protected virtual void UpdateParameters()
        {
            CalculateConstantOfRepulsion();
            CalculateConstantOfAttraction();
        }

        private void CalculateConstantOfRepulsion()
        {
            ConstantOfRepulsion = Math.Pow(K * _repulsiveMultiplier, 2);
            OnPropertyChanged(nameof(ConstantOfRepulsion));
        }

        private void CalculateConstantOfAttraction()
        {
            ConstantOfAttraction = K * _attractionMultiplier;
            OnPropertyChanged(nameof(ConstantOfAttraction));
        }

        /// <summary>
        /// Gets the computed ideal edge length.
        /// </summary>
        public abstract double K { get; }

        /// <summary>
        /// Gets the initial temperature of the mass.
        /// </summary>
        public abstract double InitialTemperature { get; }

        /// <summary>
        /// Constant. <code>Equals with K * attractionMultiplier</code>
        /// </summary>
        public double ConstantOfAttraction { get; private set; }

        /// <summary>
        /// Multiplier of the attraction. Default value is 2.
        /// </summary>
        public double AttractionMultiplier
        {
            get => _attractionMultiplier;
            set
            {
                if (SetProperty(ref _attractionMultiplier, value))
                {
                    CalculateConstantOfAttraction();
                }
            }
        }

        /// <summary>
        /// Constant. Equals with <code>Pow(K * repulsiveMultiplier, 2)</code>
        /// </summary>
        public double ConstantOfRepulsion { get; private set; }

        /// <summary>
        /// Multiplier of the repulsion. Default value is 1.
        /// </summary>
        public double RepulsiveMultiplier
        {
            get => _repulsiveMultiplier;
            set
            {
                if (SetProperty(ref _repulsiveMultiplier, value))
                {
                    CalculateConstantOfRepulsion();
                }
            }
        }

        /// <summary>
        /// Limit of the iterations. Default value is 200.
        /// </summary>
        public int IterationLimit
        {
            get => _iterationLimit;
            set => SetProperty(ref _iterationLimit, value);
        }

        /// <summary>
        /// Lambda for the cooling. Default value is 0.95.
        /// </summary>
        public double Lambda
        {
            get => _lambda;
            set => SetProperty(ref _lambda, value);
        }

        /// <summary>
        /// Gets or sets the cooling function which could be Linear or Exponential.
        /// </summary>
        public FRCoolingFunction CoolingFunction
        {
            get => _coolingFunction;
            set => SetProperty(ref _coolingFunction, value);
        }

        #endregion Properties, Parameters

        /// <summary>
        /// Default constructor
        /// </summary>
        protected FRLayoutParametersBase()
        {
            //update the parameters
            UpdateParameters();
        }
    }
}