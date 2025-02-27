﻿using QuikGraph;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public partial class SugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        private class SugiEdge : TypedEdge<SugiVertex>
        {
            public bool IsLongEdge
            {
                get => DummyVertices != null;
                set
                {
                    if (IsLongEdge != value)
                    {
                        DummyVertices = value ? new List<SugiVertex>() : null;
                    }
                }
            }

            public IList<SugiVertex> DummyVertices { get; private set; }
            public TEdge Original { get; private set; }
            public bool IsReverted => !Original.Equals(default(TEdge)) && Original.Source == Target.Original && Original.Target == Source.Original;

            public SugiEdge(TEdge original, SugiVertex source, SugiVertex target, EdgeTypes type)
                : base(source, target, type)
            {
                Original = original;
            }
        }
    }
}