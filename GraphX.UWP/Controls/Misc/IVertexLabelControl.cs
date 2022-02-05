namespace GraphX.Controls
{
    public interface IVertexLabelControl
    {
        /// <summary>
        /// Gets or sets label drawing angle in degrees
        /// </summary>
        double Angle { get; set; }

        void Hide();

        void Show();

        /// <summary>
        /// Automaticaly update vertex label position
        /// </summary>
        void UpdatePosition();
    }
}