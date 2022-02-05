namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class SimpleTreeLayoutParameters : LayoutParametersBase
    {
        private double _componentGap = 10;

        /// <summary>
        /// Gets or sets the gap between the connected components.
        /// </summary>
        public double ComponentGap
        {
            get => _componentGap;
            set => SetProperty(ref _componentGap, value);
        }

        private double _vertexGap = 10;

        /// <summary>
        /// Gets or sets the gap between the vertices.
        /// </summary>
        public double VertexGap
        {
            get => _vertexGap;
            set => SetProperty(ref _vertexGap, value);
        }

        private double _layerGap = 10;

        /// <summary>
        /// Gets or sets the gap between the layers.
        /// </summary>
        public double LayerGap
        {
            get => _layerGap;
            set => SetProperty(ref _layerGap, value);
        }

        private LayoutDirection _direction = LayoutDirection.TopToBottom;

        /// <summary>
        /// Gets or sets the direction of the layout.
        /// </summary>
        public LayoutDirection Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        private SpanningTreeGeneration _spanningTreeGeneration = SpanningTreeGeneration.DFS;

        /// <summary>
        /// Gets or sets the direction of the layout.
        /// </summary>
        public SpanningTreeGeneration SpanningTreeGeneration
        {
            get => _spanningTreeGeneration;
            set => SetProperty(ref _spanningTreeGeneration, value);
        }

        private bool _optimizeWidthAndHeight;

        public bool OptimizeWidthAndHeight
        {
            get => _optimizeWidthAndHeight;
            set => SetProperty(ref _optimizeWidthAndHeight, value);
        }

        private double _widthPerHeight = 1.0;

        public double WidthPerHeight
        {
            get => _widthPerHeight;
            set => SetProperty(ref _widthPerHeight, value);
        }
    }
}