using GraphX.Common.Models;

namespace Sandbox.Models
{
    public class DataEdge : EdgeBase<DataVertex>
    {
        private string _text;
        private string _visualcolor;
        private double _visualedgethickness;
        private double _visualedgetransparency;

        /// <summary>
        /// Custom string property
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        /// <summary>
        /// Gets or sets edge visual color
        /// </summary>
        public string VisualColor
        {
            get => _visualcolor;
            set => SetProperty(ref _visualcolor, value);
        }

        /// <summary>
        /// Gets or sets edge thickness
        /// </summary>
        public double VisualEdgeThickness
        {
            get => _visualedgethickness;
            set => SetProperty(ref _visualedgethickness, value);
        }

        /// <summary>
        /// Gets or sets edge transparency for 0 to 1
        /// </summary>
        public double VisualEdgeTransparency
        {
            get => _visualedgetransparency;
            set => SetProperty(ref _visualedgetransparency, value);
        }

        /// <summary>
        /// Default constructor. We need to set at least Source and Target properties of the edge.
        /// </summary>
        /// <param name="source">Source vertex data</param>
        /// <param name="target">Target vertex data</param>
        /// <param name="weight">Optional edge weight</param>
        public DataEdge(DataVertex source, DataVertex target, double weight = 1) : base(source, target, weight) { }

        /// <summary>
        /// Default parameterless constructor (for serialization compatibility)
        /// </summary>
        public DataEdge() : base(null, null, 1) { }

        public override string ToString()
        {
            return Text;
        }
    }
}