﻿using GraphX.Common.Interfaces;

using QuikGraph;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public partial class EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        protected class AlternatingLayer : List<IData>, ICloneable
        {
            /// <summary>
            /// This method ensures that the layer is a real alternating
            /// layer: starts with a SegmentContainer followed by a Vertex,
            /// another SegmentContainer, another Vertex, ... ending wiht
            /// a SegmentContainer.
            /// </summary>
            public void EnsureAlternatingAndPositions()
            {
                bool shouldBeAContainer = true;
                for (int i = 0; i < Count; i++, shouldBeAContainer = !shouldBeAContainer)
                {
                    if (shouldBeAContainer && this[i] is SugiVertex)
                    {
                        Insert(i, new SegmentContainer());
                    }
                    else
                    {
                        while (i < Count && !shouldBeAContainer && this[i] is SegmentContainer)
                        {
                            //the previous one must be a container too
                            SegmentContainer prevContainer = this[i - 1] as SegmentContainer;
                            SegmentContainer actualContainer = this[i] as SegmentContainer;
                            prevContainer.Join(actualContainer);
                            RemoveAt(i);
                        }
                        if (i >= Count)
                        {
                            break;
                        }
                    }
                }

                if (shouldBeAContainer)
                {
                    //the last element in the alternating layer
                    //should be a container, but it's not
                    //so add an empty one
                    Add(new SegmentContainer());
                }
            }

            protected void EnsurePositions()
            {
                //assign positions to vertices on the actualLayer (L_i)
                for (int i = 1; i < Count; i += 2)
                {
                    SegmentContainer precedingContainer = this[i - 1] as SegmentContainer;
                    SugiVertex vertex = this[i] as SugiVertex;
                    if (i == 1)
                    {
                        vertex.Position = precedingContainer.Count;
                    }
                    else
                    {
                        SugiVertex previousVertex = this[i - 2] as SugiVertex;
                        vertex.Position = previousVertex.Position + precedingContainer.Count + 1;
                    }
                }

                //assign positions to containers on the actualLayer (L_i+1)
                for (int i = 0; i < Count; i += 2)
                {
                    SegmentContainer container = this[i] as SegmentContainer;
                    if (i == 0)
                    {
                        container.Position = 0;
                    }
                    else
                    {
                        SugiVertex precedingVertex = this[i - 1] as SugiVertex;
                        container.Position = precedingVertex.Position + 1;
                    }
                }
            }

            public void SetPositions()
            {
                int nextPosition = 0;
                for (int i = 0; i < Count; i++)
                {
                    SegmentContainer segmentContainer = this[i] as SegmentContainer;
                    SugiVertex vertex = this[i] as SugiVertex;
                    if (segmentContainer != null)
                    {
                        segmentContainer.Position = nextPosition;
                        nextPosition += segmentContainer.Count;
                    }
                    else if (vertex != null)
                    {
                        vertex.Position = nextPosition;
                        nextPosition += 1;
                    }
                }
            }

            public AlternatingLayer Clone()
            {
                AlternatingLayer clonedLayer = new AlternatingLayer();
                foreach (IData item in this)
                {
                    ICloneable cloneableItem = item as ICloneable;
                    if (cloneableItem != null)
                    {
                        clonedLayer.Add(cloneableItem.Clone() as IData);
                    }
                    else
                    {
                        clonedLayer.Add(item);
                    }
                }
                return clonedLayer;
            }

            #region ICloneable Members

            object ICloneable.Clone()
            {
                return Clone();
            }

            #endregion ICloneable Members
        }
    }
}