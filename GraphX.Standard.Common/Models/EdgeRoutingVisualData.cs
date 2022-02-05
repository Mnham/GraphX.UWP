using GraphX.Common.Interfaces;
using GraphX.Measure;

namespace GraphX.Common.Models
{
    public class EdgeRoutingVisualData
    {
        public bool HaveTemplate { get; set; }
        public bool IsEdgeVisible { get; set; }
        public bool IsEdgeSelfLooped { get; set; }

        public Point SourcePosition { get; set; }
        public Size SourceSize { get; set; }
        public Point TargetPosition { get; set; }
        public Size TargetSize { get; set; }

        public IRoutingInfo Edge { get; set; }
    }
}