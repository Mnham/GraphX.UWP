using GraphX.Common.Models;

namespace Sandbox.Models
{
    public class DataVertex : VertexBase
    {
        private string _labeltext;
        private int _visualdiameter;
        private int _visualinnerdiameter;
        private bool _visualselected;

        /// <summary>
        /// Vertex label text
        /// </summary>
        public string LabelText
        {
            get => _labeltext;
            set => SetProperty(ref _labeltext, value);
        }

        /// <summary>
        /// Controls overall vertex diameter
        /// </summary>
        public int VisualDiameter
        {
            get => _visualdiameter;
            set => SetProperty(ref _visualdiameter, value);
        }

        /// <summary>
        /// Controls inner circle diameter
        /// </summary>
        public int VisualInnerDiameter
        {
            get => _visualinnerdiameter;
            set => SetProperty(ref _visualinnerdiameter, value);
        }

        /// <summary>
        /// Controls the thickness of vertex outer ring
        /// </summary>
        public double VisualOuterRingThickness => 5d;

        public bool VisualSelected
        {
            get => _visualselected;
            set => SetProperty(ref _visualselected, value);
        }

        /// <summary>
        /// Default parameterless constructor for this class
        /// (required for YAXLib serialization)
        /// </summary>
        public DataVertex() : this("") { }

        public DataVertex(string text = "")
        {
            LabelText = text;
        }

        public override string ToString()
        {
            return LabelText;
        }
    }
}