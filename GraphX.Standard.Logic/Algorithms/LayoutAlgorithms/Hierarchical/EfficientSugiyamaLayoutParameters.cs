namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public class EfficientSugiyamaLayoutParameters : LayoutParametersBase
    {
        private LayoutDirection _direction = LayoutDirection.TopToBottom;
        private double _layerDistance = 15.0;
        private double _vertexDistance = 15.0;
        private int _positionMode = -1;
        private bool _optimizeWidth;
        private double _widthPerHeight = 1.0;
        private bool _minimizeEdgeLength = true;
        internal const int MAX_PERMUTATIONS = 50;
        private SugiyamaEdgeRoutings _edgeRouting = SugiyamaEdgeRoutings.Orthogonal;

        /// <summary>
        /// Layout direction
        /// </summary>
        public LayoutDirection Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        public double LayerDistance
        {
            get => _layerDistance;
            set => SetProperty(ref _layerDistance, value);
        }

        public double VertexDistance
        {
            get => _vertexDistance;
            set => SetProperty(ref _vertexDistance, value);
        }

        public int PositionMode
        {
            get => _positionMode;
            set => SetProperty(ref _positionMode, value);
        }

        public double WidthPerHeight
        {
            get => _widthPerHeight;
            set => SetProperty(ref _widthPerHeight, value);
        }

        public bool OptimizeWidth
        {
            get => _optimizeWidth;
            set => SetProperty(ref _optimizeWidth, value);
        }

        public bool MinimizeEdgeLength
        {
            get => _minimizeEdgeLength;
            set => SetProperty(ref _minimizeEdgeLength, value);
        }

        public SugiyamaEdgeRoutings EdgeRouting
        {
            get => _edgeRouting;
            set => SetProperty(ref _edgeRouting, value);
        }
    }
}