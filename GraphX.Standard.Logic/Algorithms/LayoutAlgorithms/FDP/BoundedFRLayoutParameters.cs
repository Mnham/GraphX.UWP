using System;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    /// <summary>
    /// Parameters of the Fruchterman-Reingold Algorithm (FDP), bounded version.
    /// </summary>
    public class BoundedFRLayoutParameters : FRLayoutParametersBase
    {
        #region Properties, Parameters

        //some of the parameters declared with 'internal' modifier to 'speed up'

        private double _width = 1000;
        private double _height = 1000;
        private double _k;

        /// <summary>
        /// Width of the bounding box.
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                if (SetProperty(ref _width, value))
                {
                    UpdateParameters();
                }
            }
        }

        /// <summary>
        /// Height of the bounding box.
        /// </summary>
        public double Height
        {
            get => _height;
            set
            {
                if (SetProperty(ref _height, value))
                {
                    UpdateParameters();
                }
            }
        }

        /// <summary>
        /// Constant. <code>IdealEdgeLength = sqrt(height * width / vertexCount)</code>
        /// </summary>
        public override double K => _k;

        /// <summary>
        /// Gets the initial temperature of the mass.
        /// </summary>
        public override double InitialTemperature => Math.Min(Width, Height) / 10;

        protected override void UpdateParameters()
        {
            _k = Math.Sqrt(_width * Height / VertexCount);
            OnPropertyChanged(nameof(K));
            base.UpdateParameters();
        }

        #endregion Properties, Parameters
    }
}