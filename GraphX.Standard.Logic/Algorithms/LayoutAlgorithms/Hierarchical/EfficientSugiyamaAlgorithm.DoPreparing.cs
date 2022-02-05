using QuikGraph;
using QuikGraph.Algorithms.Search;

using System.Collections.Generic;
using System.Linq;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public partial class EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        private void DoPreparing()
        {
            RemoveCycles();
            RemoveLoops();
            RemoveIsolatedVertices(); //it must run after the two method above
        }

        private void RemoveIsolatedVertices()
        {
            _isolatedVertices = _graph.Vertices.Where(v => _graph.Degree(v) == 0).ToList();
            foreach (SugiVertex isolatedVertex in _isolatedVertices)
            {
                _graph.RemoveVertex(isolatedVertex);
            }
        }

        /// <summary>
        /// Removes the edges which source and target is the same vertex.
        /// </summary>
        private void RemoveLoops()
        {
            _graph.RemoveEdgeIf(edge => edge.Source == edge.Target);
        }

        /// <summary>
        /// Removes the cycles from the original graph with simply reverting
        /// some edges.
        /// </summary>
        private void RemoveCycles()
        {
            //find the cycle edges with dfs
            List<SugiEdge> cycleEdges = new List<SugiEdge>();
            DepthFirstSearchAlgorithm<SugiVertex, SugiEdge> dfsAlgo = new DepthFirstSearchAlgorithm<SugiVertex, SugiEdge>(_graph);
            dfsAlgo.BackEdge += cycleEdges.Add;
            dfsAlgo.Compute();

            //and revert them
            foreach (SugiEdge edge in cycleEdges)
            {
                _graph.RemoveEdge(edge);
                _graph.AddEdge(new SugiEdge(edge.OriginalEdge, edge.Target, edge.Source));
            }
        }
    }
}