namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public enum PositionCalculationMethodTypes
    {
        /// <summary>
        /// Barycenters of the vertices computed based on the
        /// indexes of the vertices.
        /// </summary>
        IndexBased,

        /// <summary>
        /// Barycenters of the vertices computed based on
        /// the vertex sizes and positions.
        /// </summary>
        PositionBased
    }

    /// <summary>
    /// Parameters of the Sugiyama layout.
    /// </summary>
    public class SugiyamaLayoutParameters : LayoutParametersBase
    {
        #region Helper Types

        public enum PromptingConstraintType
        {
            Compulsory,
            Recommendation,
            Irrelevant
        }

        #endregion Helper Types

        internal float Horizontalgap = 10;
        internal float Verticalgap = 10;
        private bool _dirty = true;
        private int _phase1IterationCount = 8;
        private int _phase2IterationCount = 5;
        private bool _minimizeHierarchicalEdgeLong = true;
        private PositionCalculationMethodTypes _positionCalculationMethod = PositionCalculationMethodTypes.PositionBased;
        private bool _simplify = true;
        private bool _baryCenteringByPosition;
        private PromptingConstraintType _promptingConstraint = PromptingConstraintType.Recommendation;
        private float _maxWidth = float.MaxValue;

        /// <summary>
        /// Minimal horizontal gap between the vertices.
        /// </summary>
        public float HorizontalGap
        {
            get => Horizontalgap;
            set => SetProperty(ref Horizontalgap, value);
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set => SetProperty(ref _maxWidth, value);
        }

        public bool BaryCenteringByPosition
        {
            get => _baryCenteringByPosition;
            set => SetProperty(ref _baryCenteringByPosition, value);
        }

        /// <summary>
        /// Minimal vertical gap between the vertices.
        /// </summary>
        public float VerticalGap
        {
            get => Verticalgap;
            set => SetProperty(ref Verticalgap, value);
        }

        /// <summary>
        /// Start with a dirty round (allow to increase the number of the edge-crossings, but
        /// try to put the vertices to it's barycenter).
        /// </summary>
        public bool DirtyRound
        {
            get => _dirty;
            set => SetProperty(ref _dirty, value);
        }

        /// <summary>
        /// Maximum iteration count in the Phase 1 of the Sugiyama algo.
        /// </summary>
        public int Phase1IterationCount
        {
            get => _phase1IterationCount;
            set => SetProperty(ref _phase1IterationCount, value);
        }

        /// <summary>
        /// Maximum iteration count in the Phase 2 of the Sugiyama algo.
        /// </summary>
        public int Phase2IterationCount
        {
            get => _phase2IterationCount;
            set => SetProperty(ref _phase2IterationCount, value);
        }

        public bool MinimizeHierarchicalEdgeLong
        {
            get => _minimizeHierarchicalEdgeLong;
            set => SetProperty(ref _minimizeHierarchicalEdgeLong, value);
        }

        public PositionCalculationMethodTypes PositionCalculationMethod
        {
            get => _positionCalculationMethod;
            set => SetProperty(ref _positionCalculationMethod, value);
        }

        /// <summary>
        /// Gets or sets the 'Simplify' parameter.
        /// If true than the edges which directly goes to a vertex which could
        /// be reached on another path (which is not directly goes to that vertex, there's some plus vertices)
        /// will not be count in the layout algorithm.
        /// </summary>
        public bool Simplify
        {
            get => _simplify;
            set => SetProperty(ref _simplify, value);
        }

        /// <summary>
        /// Prompting constraint type for the starting positions.
        /// </summary>
        public PromptingConstraintType Prompting
        {
            get => _promptingConstraint;
            set => SetProperty(ref _promptingConstraint, value);
        }
    }
}