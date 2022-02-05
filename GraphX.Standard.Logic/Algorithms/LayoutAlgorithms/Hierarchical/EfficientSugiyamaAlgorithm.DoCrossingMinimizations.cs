using GraphX.Common;

using QuikGraph;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GraphX.Logic.Algorithms.LayoutAlgorithms
{
    public partial class EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>
        where TVertex : class
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexAndEdgeListGraph<TVertex, TEdge>, IMutableVertexAndEdgeSet<TVertex, TEdge>
    {
        private int[] _crossCounts;

        private IList<Edge<Data>>[] _sparseCompactionByLayerBackup;

        private AlternatingLayer[] _alternatingLayers;

        /// <summary>
        /// Minimizes the crossings between the layers by sweeping up and down
        /// while there could be something optimized.
        /// </summary>
        private void DoCrossingMinimizations(CancellationToken cancellationToken)
        {
            int prevCrossings = int.MaxValue;
            int crossings = int.MaxValue;
            int phase = 1;

            _crossCounts = new int[_layers.Count];
            _sparseCompactionByLayerBackup = new IList<Edge<Data>>[_layers.Count];
            _alternatingLayers = new AlternatingLayer[_layers.Count];
            for (int i = 0; i < _layers.Count; i++)
            {
                _crossCounts[i] = int.MaxValue;
            }

            int phase1iterationLeft = 100;
            int phase2iterationLeft = _layers.Count;
            bool enableSameMeasureOptimization = true;
            bool changed = false;
            bool c = false;
            bool wasPhase2 = false;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                changed = false;
                prevCrossings = crossings;
                if (phase == 1)
                {
                    phase1iterationLeft--;
                }
                else if (phase == 2)
                {
                    phase2iterationLeft--;
                }

                wasPhase2 = (phase == 2);

                crossings = Sweeping(0, _layers.Count - 1, 1, enableSameMeasureOptimization, out c, ref phase, cancellationToken);
                changed = changed || c;
                if (crossings == 0)
                {
                    break;
                }

                crossings = Sweeping(_layers.Count - 1, 0, -1, enableSameMeasureOptimization, out c, ref phase, cancellationToken);
                changed = changed || c;
                if (phase == 1 && (!changed || crossings >= prevCrossings) && phase2iterationLeft > 0)
                {
                    phase = 2;
                }
                else if (phase == 2)
                {
                    phase = 1;
                }
            } while (crossings > 0
                && ((phase2iterationLeft > 0 || wasPhase2)
                    || (phase1iterationLeft > 0 && (crossings < prevCrossings) && changed)));
        }

        /// <summary>
        /// Sweeps between the <paramref name="startLayerIndex"/> and <paramref name="endLayerIndex"/>
        /// in the way represented by the step
        /// </summary>
        /// <param name="startLayerIndex">The index of the start layer (where the sweeping starts from).</param>
        /// <param name="endLayerIndex">The index of the last layer (where the sweeping ends).</param>
        /// <param name="step">Increment or decrement of the layer index. (1 or -1)</param>
        /// <returns>The number of the edge crossings.</returns>
        private int Sweeping(int startLayerIndex, int endLayerIndex, int step, bool enableSameMeasureOptimization, out bool changed, ref int phase, CancellationToken cancellationToken)
        {
            int crossings = 0;
            changed = false;
            AlternatingLayer alternatingLayer = null;
            if (_alternatingLayers.Length == 0)
            {
                return 0;
            }

            if (_alternatingLayers[startLayerIndex] == null)
            {
                alternatingLayer = new AlternatingLayer();
                alternatingLayer.AddRange(_layers[startLayerIndex].OfType<IData>());
                alternatingLayer.EnsureAlternatingAndPositions();
                AddAlternatingLayerToSparseCompactionGraph(alternatingLayer, startLayerIndex);
                _alternatingLayers[startLayerIndex] = alternatingLayer;
            }
            else
            {
                alternatingLayer = _alternatingLayers[startLayerIndex];
            }
            OutputAlternatingLayer(alternatingLayer, startLayerIndex, 0);
            for (int i = startLayerIndex; i != endLayerIndex; i += step)
            {
                int ci = Math.Min(i, i + step);
                int prevCrossCount = _crossCounts[ci];

                if (_alternatingLayers[i + step] != null)
                {
                    alternatingLayer.SetPositions();
                    _alternatingLayers[i + step].SetPositions();
                    prevCrossCount = DoCrossCountingAndOptimization(alternatingLayer, _alternatingLayers[i + step], (i < i + step), false, (phase == 2), int.MaxValue, cancellationToken);
                    _crossCounts[ci] = prevCrossCount;
                }

                int crossCount = CrossingMinimizationBetweenLayers(ref alternatingLayer, i, i + step, enableSameMeasureOptimization, prevCrossCount, phase, cancellationToken);

                if (crossCount < prevCrossCount || phase == 2 || changed)
                {
                    /* set the sparse compaction graph */
                    AddAlternatingLayerToSparseCompactionGraph(alternatingLayer, i + step);
                    ReplaceLayer(alternatingLayer, i + step);
                    _alternatingLayers[i + step] = alternatingLayer;
                    OutputAlternatingLayer(alternatingLayer, i + step, crossCount);
                    _crossCounts[i] = crossCount;
                    crossings += crossCount;
                    changed = true;
                    /*if (phase == 2)
                    {
                        Debug.WriteLine("Phase changed on layer " + (i + step));
                        phase = 1;
                    }*/
                }
                else
                {
                    //!PCL-NON-COMPL! Debug.WriteLine("Layer " + (i + step) + " has not changed.");
                    alternatingLayer = _alternatingLayers[i + step];
                    crossings += prevCrossCount;
                }
            }
            return crossings;
        }

        [Conditional("DEBUG")]
        private void OutputAlternatingLayer(AlternatingLayer alternatingLayer, int layerIndex, int crossCount)
        {
            /*!PCL-NON-COMPL!  //Debug.Write(layerIndex + " | " + crossCount + ": ");
              for (int i = 0; i < alternatingLayer.Count; i++)
              {
                  if (alternatingLayer[i] is SugiVertex)
                  {
                      var vertex = alternatingLayer[i] as SugiVertex;
                      Debug.Write(string.Format("{0},{1}\t", vertex.OriginalVertex, vertex.Type.ToString()[0]));
                  }
                  else
                  {
                      var segmentContainer = alternatingLayer[i] as SegmentContainer;
                      for (int j = 0; j < segmentContainer.Count; j++)
                      {
                          Debug.Write("| \t");
                      }
                  }
              }
              Debug.WriteLine("");*/
        }

        private void ReplaceLayer(AlternatingLayer alternatingLayer, int i)
        {
            _layers[i].Clear();
            foreach (IData item in alternatingLayer)
            {
                SugiVertex vertex = item as SugiVertex;
                if (vertex == null)
                {
                    continue;
                }

                _layers[i].Add(vertex);
                vertex.IndexInsideLayer = i;
            }
        }

        private int CrossingMinimizationBetweenLayers(
            ref AlternatingLayer alternatingLayer,
            int actualLayerIndex,
            int nextLayerIndex,
            bool enableSameMeasureOptimization,
            int prevCrossCount,
            int phase,
            CancellationToken cancellationToken)
        {
            //decide which way we are sweeping (up or down)
            //straight = down, reverse = up
            bool straightSweep = (actualLayerIndex < nextLayerIndex);
            AlternatingLayer nextAlternatingLayer = alternatingLayer.Clone();

            /* 1 */
            AppendSegmentsToAlternatingLayer(nextAlternatingLayer, straightSweep);

            /* 2 */
            ComputeMeasureValues(alternatingLayer, nextLayerIndex, straightSweep, cancellationToken);
            nextAlternatingLayer.SetPositions();

            /* 3 */
            nextAlternatingLayer = InitialOrderingOfNextLayer(nextAlternatingLayer, _layers[nextLayerIndex], straightSweep);

            /* 4 */
            PlaceQVertices(nextAlternatingLayer, _layers[nextLayerIndex], straightSweep, cancellationToken);
            nextAlternatingLayer.SetPositions();

            /* 5 */
            int crossCount = DoCrossCountingAndOptimization(alternatingLayer, nextAlternatingLayer, straightSweep, enableSameMeasureOptimization, (phase == 2), prevCrossCount, cancellationToken);

            /* 6 */
            nextAlternatingLayer.EnsureAlternatingAndPositions();

            alternatingLayer = nextAlternatingLayer;
            return crossCount;
        }

        private IList<SugiVertex> FindVerticesWithSameMeasure(CancellationToken cancellationToken, AlternatingLayer nextAlternatingLayer, bool straightSweep, out IList<int> ranges, out int maxRangeLength)
        {
            VertexTypes ignorableVertexType = straightSweep ? VertexTypes.QVertex : VertexTypes.PVertex;
            List<SugiVertex> verticesWithSameMeasure = new List<SugiVertex>();
            SugiVertex[] vertices = nextAlternatingLayer.OfType<SugiVertex>().ToArray();
            int startIndex, endIndex;
            maxRangeLength = 0;
            int rangeCount = 0;
            ranges = new List<int>();
            for (startIndex = 0; startIndex < vertices.Length; startIndex = endIndex + 1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                for (endIndex = startIndex + 1;
                      endIndex < vertices.Length && vertices[startIndex].MeasuredPosition == vertices[endIndex].MeasuredPosition;
                      endIndex++)
                { }
                endIndex -= 1;

                if (endIndex > startIndex)
                {
                    int rangeLength = 0;
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (vertices[i].Type == ignorableVertexType || vertices[i].DoNotOpt)
                        {
                            continue;
                        }

                        rangeLength++;
                        verticesWithSameMeasure.Add(vertices[i]);
                    }
                    if (rangeLength > 0)
                    {
                        maxRangeLength = Math.Max(rangeLength, maxRangeLength);
                        ranges.Add(rangeLength);
                        rangeCount++;
                    }
                }
            }
            return verticesWithSameMeasure;
        }

        private void AddAlternatingLayerToSparseCompactionGraph(AlternatingLayer nextAlternatingLayer, int layerIndex)
        {
            IList<Edge<Data>> _sparseCompationGraphEdgesOfLayer = _sparseCompactionByLayerBackup[layerIndex];
            if (_sparseCompationGraphEdgesOfLayer != null)
            {
                foreach (Edge<Data> edge in _sparseCompationGraphEdgesOfLayer)
                {
                    _sparseCompactionGraph.RemoveEdge(edge);
                }
            }

            _sparseCompationGraphEdgesOfLayer = new List<Edge<Data>>();
            SugiVertex vertex = null, prevVertex = null;
            for (int i = 1; i < nextAlternatingLayer.Count; i += 2)
            {
                vertex = nextAlternatingLayer[i] as SugiVertex;
                SegmentContainer prevContainer = nextAlternatingLayer[i - 1] as SegmentContainer;
                SegmentContainer nextContainer = nextAlternatingLayer[i + 1] as SegmentContainer;
                if (prevContainer != null && prevContainer.Count > 0)
                {
                    Segment lastSegment = prevContainer[prevContainer.Count - 1];
                    Edge<Data> edge = new Edge<Data>(lastSegment, vertex);
                    _sparseCompationGraphEdgesOfLayer.Add(edge);
                    _sparseCompactionGraph.AddVerticesAndEdge(edge);
                }
                else if (prevVertex != null)
                {
                    Edge<Data> edge = new Edge<Data>(prevVertex, vertex);
                    _sparseCompationGraphEdgesOfLayer.Add(edge);
                    _sparseCompactionGraph.AddVerticesAndEdge(edge);
                }

                if (nextContainer != null && nextContainer.Count > 0)
                {
                    Segment firstSegment = nextContainer[0];
                    Edge<Data> edge = new Edge<Data>(vertex, firstSegment);
                    _sparseCompationGraphEdgesOfLayer.Add(edge);
                    _sparseCompactionGraph.AddVerticesAndEdge(edge);
                }

                if (vertex != null && !_sparseCompactionGraph.ContainsVertex(vertex))
                {
                    _sparseCompactionGraph.AddVertex(vertex);
                }

                prevVertex = vertex;
            }
            _sparseCompactionByLayerBackup[layerIndex] = _sparseCompationGraphEdgesOfLayer;
        }

        private class VertexGroup
        {
            public int Position;
            public int Size;
        }

        private class CrossCounterPair : Pair
        {
            public EdgeTypes Type = EdgeTypes.InnerSegment;
            public SugiEdge NonInnerSegment = null;
        }

        private class CrossCounterTreeNode
        {
            public int Accumulator;
            public bool InnerSegmentMarker;
            public readonly Queue<SugiEdge> NonInnerSegmentQueue = new Queue<SugiEdge>();
        }

        private int DoCrossCountingAndOptimization(
            AlternatingLayer alternatingLayer,
            AlternatingLayer nextAlternatingLayer,
            bool straightSweep,
            bool enableSameMeasureOptimization,
            bool reverseVerticesWithSameMeasure,
            int prevCrossCount,
            CancellationToken cancellationToken)
        {
            IList<CrossCounterPair> realEdgePairs;
            AlternatingLayer topLayer = straightSweep ? alternatingLayer : nextAlternatingLayer;
            AlternatingLayer bottomLayer = straightSweep ? nextAlternatingLayer : alternatingLayer;

            int firstLayerSize, secondLayerSize;
            IData lastOnTopLayer = topLayer[topLayer.Count - 1];
            IData lastOnBottomLayer = bottomLayer[bottomLayer.Count - 1];
            firstLayerSize = lastOnTopLayer.Position + (lastOnTopLayer is ISegmentContainer ? ((ISegmentContainer)lastOnTopLayer).Count : 1);
            secondLayerSize = lastOnBottomLayer.Position + (lastOnBottomLayer is ISegmentContainer ? ((ISegmentContainer)lastOnBottomLayer).Count : 1);

            IList<CrossCounterPair> virtualEdgePairs = FindVirtualEdgePairs(topLayer, bottomLayer);
            IList<SugiEdge> realEdges = FindRealEdges(topLayer, cancellationToken);

            if (enableSameMeasureOptimization || reverseVerticesWithSameMeasure)
            {
                IList<SugiVertex> verticesWithSameMeasure = FindVerticesWithSameMeasure(cancellationToken, nextAlternatingLayer, straightSweep, out IList<int> ranges, out int maxRangeLength);
                HashSet<SugiVertex> verticesWithSameMeasureSet = new HashSet<SugiVertex>(verticesWithSameMeasure);

                //initialize permutation indices
                for (int i = 0; i < verticesWithSameMeasure.Count; i++)
                {
                    verticesWithSameMeasure[i].PermutationIndex = i;
                }

                int bestCrossCount = prevCrossCount;
                foreach (SugiEdge realEdge in realEdges)
                {
                    realEdge.SaveMarkedToTemp();
                }

                List<SugiVertex> sortedVertexList = null;
                if (!reverseVerticesWithSameMeasure)
                {
                    sortedVertexList = new List<SugiVertex>(verticesWithSameMeasure);
                }
                else
                {
                    sortedVertexList = new List<SugiVertex>(verticesWithSameMeasure.Count);
                    Stack<SugiVertex> stack = new Stack<SugiVertex>(verticesWithSameMeasure.Count);
                    Random rnd = new Random(Parameters.Seed);
                    foreach (SugiVertex v in verticesWithSameMeasure)
                    {
                        if (stack.Count > 0 && (stack.Peek().MeasuredPosition != v.MeasuredPosition || rnd.NextDouble() > 0.8))
                        {
                            while (stack.Count > 0)
                            {
                                sortedVertexList.Add(stack.Pop());
                            }
                        }
                        stack.Push(v);
                    }
                    while (stack.Count > 0)
                    {
                        sortedVertexList.Add(stack.Pop());
                    }
                }

                int maxPermutations = EfficientSugiyamaLayoutParameters.MAX_PERMUTATIONS;
                do
                {
                    maxPermutations--;
                    if (!reverseVerticesWithSameMeasure)
                    {
                        //sort by permutation index and measure
                        sortedVertexList.Sort((v1, v2) =>
                        {
                            if (v1.MeasuredPosition != v2.MeasuredPosition)
                            {
                                return Math.Sign(v1.MeasuredPosition - v2.MeasuredPosition);
                            }

                            return v1.PermutationIndex - v2.PermutationIndex;
                        });
                    }

                    //reinsert the vertices into the layer
                    ReinsertVerticesIntoLayer(nextAlternatingLayer, verticesWithSameMeasureSet, sortedVertexList);

                    //set the positions
                    nextAlternatingLayer.SetPositions();
                    realEdgePairs = ConvertRealEdgesToCrossCounterPairs(realEdges, true);

                    List<CrossCounterPair> edgePairs = new List<CrossCounterPair>();
                    edgePairs.AddRange(virtualEdgePairs);
                    edgePairs.AddRange(realEdgePairs);

                    int crossCount = BiLayerCrossCount(edgePairs, firstLayerSize, secondLayerSize, cancellationToken);

                    if (reverseVerticesWithSameMeasure)
                    {
                        return crossCount;
                    }

                    //if the crosscount is better than the best known
                    //save the actual state
                    if (crossCount < bestCrossCount)
                    {
                        foreach (SugiVertex vertex in verticesWithSameMeasure)
                        {
                            vertex.SavePositionToTemp();
                        }

                        foreach (SugiEdge edge in realEdges)
                        {
                            edge.SaveMarkedToTemp();
                        }

                        bestCrossCount = crossCount;
                    }
                    if (crossCount == 0)
                    {
                        break;
                    }
                } while (maxPermutations > 0 && Permutate(verticesWithSameMeasure, ranges));

                //reload the best solution
                foreach (SugiVertex vertex in verticesWithSameMeasure)
                {
                    vertex.LoadPositionFromTemp();
                }

                foreach (SugiEdge edge in realEdges)
                {
                    edge.LoadMarkedFromTemp();
                }

                //sort by permutation index and measure
                sortedVertexList.Sort((v1, v2) => v1.Position - v2.Position);

                //reinsert the vertices into the layer
                ReinsertVerticesIntoLayer(nextAlternatingLayer, verticesWithSameMeasureSet, sortedVertexList);
                nextAlternatingLayer.SetPositions();

                return bestCrossCount;
            }
            realEdgePairs = ConvertRealEdgesToCrossCounterPairs(realEdges, true);
            List<CrossCounterPair> fEdgePairs = new List<CrossCounterPair>();
            fEdgePairs.AddRange(virtualEdgePairs);
            fEdgePairs.AddRange(realEdgePairs);

            return BiLayerCrossCount(fEdgePairs, firstLayerSize, secondLayerSize, cancellationToken);
        }

        private static void ReinsertVerticesIntoLayer(
            AlternatingLayer layer,
            HashSet<SugiVertex> vertexSet,
            IList<SugiVertex> vertexList)
        {
            int reinsertIndex = 0;
            for (int i = 0; i < layer.Count; i++)
            {
                SugiVertex vertex = layer[i] as SugiVertex;
                if (vertex == null || !vertexSet.Contains(vertex))
                {
                    continue;
                }

                layer.RemoveAt(i);
                layer.Insert(i, vertexList[reinsertIndex]);
                reinsertIndex++;
            }
        }

        private IList<CrossCounterPair> ConvertRealEdgesToCrossCounterPairs(IList<SugiEdge> edges, bool clearMark)
        {
            List<CrossCounterPair> pairs = new List<CrossCounterPair>();
            foreach (SugiEdge edge in edges)
            {
                SugiVertex source = edge.Source;
                SugiVertex target = edge.Target;
                pairs.Add(
                    new CrossCounterPair
                    {
                        First = source.Position,
                        Second = target.Position,
                        Weight = 1,
                        Type = EdgeTypes.NonInnerSegment,
                        NonInnerSegment = edge
                    });

                if (clearMark)
                {
                    edge.Marked = false;
                }
            }
            return pairs;
        }

        private IList<SugiEdge> FindRealEdges(AlternatingLayer topLayer, CancellationToken cancellationToken)
        {
            List<SugiEdge> realEdges = new List<SugiEdge>();
            foreach (IData item in topLayer)
            {
                SugiVertex vertex = item as SugiVertex;
                if (vertex == null || vertex.Type == VertexTypes.PVertex)
                {
                    continue;
                }

                foreach (SugiEdge edge in _graph.OutEdges(vertex))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    realEdges.Add(edge);
                }
            }
            return realEdges;
        }

        private IList<CrossCounterPair> FindVirtualEdgePairs(AlternatingLayer topLayer, AlternatingLayer bottomLayer)
        {
            List<CrossCounterPair> virtualEdgePairs = new List<CrossCounterPair>();
            Queue<VertexGroup> firstLayerQueue = GetContainerLikeItems(topLayer, VertexTypes.PVertex);
            Queue<VertexGroup> secondLayerQueue = GetContainerLikeItems(bottomLayer, VertexTypes.QVertex);
            VertexGroup vg1, vg2;
            vg1 = new VertexGroup();
            vg2 = new VertexGroup();
            while (firstLayerQueue.Count > 0 || secondLayerQueue.Count > 0)
            {
                if (vg1.Size == 0)
                {
                    vg1 = firstLayerQueue.Dequeue();
                }

                if (vg2.Size == 0)
                {
                    vg2 = secondLayerQueue.Dequeue();
                }

                if (vg1.Size <= vg2.Size)
                {
                    virtualEdgePairs.Add(
                        new CrossCounterPair
                        {
                            First = vg1.Position,
                            Second = vg2.Position,
                            Weight = vg1.Size
                        });
                    vg2.Size -= vg1.Size;
                    vg1.Size = 0;
                }
                else
                {
                    virtualEdgePairs.Add(
                        new CrossCounterPair
                        {
                            First = vg1.Position,
                            Second = vg2.Position,
                            Weight = vg2.Size
                        });
                    vg1.Size -= vg2.Size;
                    vg2.Size = 0;
                }
            }
            return virtualEdgePairs;
        }

        private bool Permutate(IList<SugiVertex> vertices, IList<int> ranges)
        {
            Random rnd = new Random(Parameters.Seed);
            bool b = false;
            for (int i = 0, startIndex = 0; i < ranges.Count; startIndex += ranges[i], i++)
            {
                b = b || PermutateSomeHow(vertices, startIndex, ranges[i], rnd);
            }
            return b;
        }

        private bool PermutateSomeHow(IList<SugiVertex> vertices, int startIndex, int count, Random rnd)
        {
            if (count <= 4)
            {
                return Permutate(vertices, startIndex, count);
            }
            else
            {
                return PermutateRandom(vertices, startIndex, count, rnd);
            }
        }

        private bool PermutateRandom(IList<SugiVertex> vertices, int startIndex, int count, Random rnd)
        {
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                vertices[i].PermutationIndex = rnd.Next(count);
            }
            return true;
        }

        private bool Permutate(IList<SugiVertex> vertices, int startIndex, int count)
        {
            //do the initial ordering
            int n = startIndex + count;
            int i, j;

            //find place to start
            for (i = n - 1;
                  i > startIndex && vertices[i - 1].PermutationIndex >= vertices[i].PermutationIndex;
                  i--)
            { }

            //all in reverse order
            if (i < startIndex + 1)
            {
                return false; //no more permutation
            }

            //do next permutation
            for (j = n;
                  j > startIndex + 1 && vertices[j - 1].PermutationIndex <= vertices[i - 1].PermutationIndex;
                  j--)
            { }

            //swap values i-1, j-1
            int c = vertices[i - 1].PermutationIndex;
            vertices[i - 1].PermutationIndex = vertices[j - 1].PermutationIndex;
            vertices[j - 1].PermutationIndex = c;

            //need more swaps
            for (i++, j = n; i < j; i++, j--)
            {
                c = vertices[i - 1].PermutationIndex;
                vertices[i - 1].PermutationIndex = vertices[j - 1].PermutationIndex;
                vertices[j - 1].PermutationIndex = c;
            }

            return true; //new permutation generated
        }

        private static int BiLayerCrossCount(IEnumerable<CrossCounterPair> pairs, int firstLayerVertexCount, int secondLayerVertexCount, CancellationToken cancellationToken)
        {
            if (pairs == null)
            {
                return 0;
            }

            //radix sort of the pair, order by First asc, Second asc

            #region Sort by Second ASC

            List<CrossCounterPair>[] radixBySecond = new List<CrossCounterPair>[secondLayerVertexCount];
            List<CrossCounterPair> r;
            int pairCount = 0;
            foreach (CrossCounterPair pair in pairs)
            {
                //get the radix where the pair should be inserted
                r = radixBySecond[pair.Second];
                if (r == null)
                {
                    r = new List<CrossCounterPair>();
                    radixBySecond[pair.Second] = r;
                }
                r.Add(pair);
                pairCount = Math.Max(pairCount, pair.Second);
            }
            pairCount += 1;

            #endregion Sort by Second ASC

            #region Sort By First ASC

            List<CrossCounterPair>[] radixByFirst = new List<CrossCounterPair>[firstLayerVertexCount];
            foreach (List<CrossCounterPair> list in radixBySecond)
            {
                if (list == null)
                {
                    continue;
                }

                foreach (CrossCounterPair pair in list)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    //get the radix where the pair should be inserted
                    r = radixByFirst[pair.First];
                    if (r == null)
                    {
                        r = new List<CrossCounterPair>();
                        radixByFirst[pair.First] = r;
                    }
                    r.Add(pair);
                }
            }

            #endregion Sort By First ASC

            //
            // Build the accumulator tree
            //
            int firstIndex = 1;
            while (firstIndex < pairCount)
            {
                firstIndex *= 2;
            }

            int treeSize = 2 * firstIndex - 1;
            firstIndex -= 1;
            CrossCounterTreeNode[] tree = new CrossCounterTreeNode[treeSize];
            for (int i = 0; i < treeSize; i++)
            {
                tree[i] = new CrossCounterTreeNode();
            }

            //
            // Count the crossings
            //
            int crossCount = 0;
            int index;
            foreach (List<CrossCounterPair> list in radixByFirst)
            {
                if (list == null)
                {
                    continue;
                }

                foreach (CrossCounterPair pair in list)
                {
                    index = pair.Second + firstIndex;
                    tree[index].Accumulator += pair.Weight;
                    switch (pair.Type)
                    {
                        case EdgeTypes.InnerSegment:
                            tree[index].InnerSegmentMarker = true;
                            break;

                        case EdgeTypes.NonInnerSegment:
                            tree[index].NonInnerSegmentQueue.Enqueue(pair.NonInnerSegment);
                            break;

                        default:
                            break;
                    }
                    while (index > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (index % 2 > 0)
                        {
                            crossCount += tree[index + 1].Accumulator * pair.Weight;
                            switch (pair.Type)
                            {
                                case EdgeTypes.InnerSegment:
                                    Queue<SugiEdge> queue = tree[index + 1].NonInnerSegmentQueue;
                                    while (queue.Count > 0)
                                    {
                                        queue.Dequeue().Marked = true;
                                    }
                                    break;

                                case EdgeTypes.NonInnerSegment:
                                    if (tree[index + 1].InnerSegmentMarker)
                                    {
                                        pair.NonInnerSegment.Marked = true;
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                        index = (index - 1) / 2;
                        tree[index].Accumulator += pair.Weight;
                        switch (pair.Type)
                        {
                            case EdgeTypes.InnerSegment:
                                tree[index].InnerSegmentMarker = true;
                                break;

                            case EdgeTypes.NonInnerSegment:
                                tree[index].NonInnerSegmentQueue.Enqueue(pair.NonInnerSegment);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            return crossCount;
        }

        private static Queue<VertexGroup> GetContainerLikeItems(AlternatingLayer alternatingLayer, VertexTypes containerLikeVertexType)
        {
            Queue<VertexGroup> queue = new Queue<VertexGroup>();
            foreach (IData item in alternatingLayer)
            {
                SugiVertex vertex = item as SugiVertex;
                if (vertex != null && vertex.Type == containerLikeVertexType)
                {
                    queue.Enqueue(new VertexGroup() { Position = vertex.Position, Size = 1 });
                }
                else if (vertex == null)
                {
                    ISegmentContainer container = item as ISegmentContainer;
                    if (container.Count > 0)
                    {
                        queue.Enqueue(new VertexGroup() { Position = container.Position, Size = container.Count });
                    }
                }
            }
            return queue;
        }

        private void PlaceQVertices(AlternatingLayer alternatingLayer, IList<SugiVertex> nextLayer, bool straightSweep, CancellationToken cancellationToken)
        {
            VertexTypes type = straightSweep ? VertexTypes.QVertex : VertexTypes.PVertex;
            HashSet<SugiVertex> qVertices = new HashSet<SugiVertex>();
            foreach (SugiVertex vertex in nextLayer)
            {
                if (vertex.Type != type)
                {
                    continue;
                }

                qVertices.Add(vertex);
            }

            for (int i = 0; i < alternatingLayer.Count; i++)
            {
                SegmentContainer segmentContainer = alternatingLayer[i] as SegmentContainer;
                if (segmentContainer == null)
                {
                    continue;
                }

                for (int j = 0; j < segmentContainer.Count; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Segment segment = segmentContainer[j];
                    SugiVertex vertex = straightSweep ? segment.QVertex : segment.PVertex;
                    if (!qVertices.Contains(vertex))
                    {
                        continue;
                    }

                    alternatingLayer.RemoveAt(i);
                    segmentContainer.Split(segment, out ISegmentContainer sc1, out ISegmentContainer sc2);
                    sc1.Position = segmentContainer.Position;
                    sc2.Position = segmentContainer.Position + sc1.Count + 1;
                    alternatingLayer.Insert(i, sc1);
                    alternatingLayer.Insert(i + 1, vertex);
                    alternatingLayer.Insert(i + 2, sc2);
                    i = i + 1;
                    break;
                }
            }
        }

        /// <summary>
        /// Replaces the P or Q vertices of the actualLayer with their
        /// segment on the next layer.
        /// </summary>
        /// <param name="alternatingLayer">The actual alternating layer. It will be modified.</param>
        /// <param name="straightSweep">If true, we are sweeping down else we're sweeping up.</param>
        private void AppendSegmentsToAlternatingLayer(AlternatingLayer alternatingLayer, bool straightSweep)
        {
            VertexTypes type = straightSweep ? VertexTypes.PVertex : VertexTypes.QVertex;
            for (int i = 1; i < alternatingLayer.Count; i += 2)
            {
                SugiVertex vertex = alternatingLayer[i] as SugiVertex;
                if (vertex.Type == type)
                {
                    SegmentContainer precedingContainer = alternatingLayer[i - 1] as SegmentContainer;
                    SegmentContainer succeedingContainer = alternatingLayer[i + 1] as SegmentContainer;
                    precedingContainer.Append(vertex.Segment);
                    precedingContainer.Join(succeedingContainer);

                    //remove the vertex and the succeeding container from the alternating layer
                    alternatingLayer.RemoveRange(i, 2);
                    i -= 2;
                }
            }
        }

        private void ComputeMeasureValues(AlternatingLayer alternatingLayer, int nextLayerIndex, bool straightSweep, CancellationToken cancellationToken)
        {
            AssignPositionsOnActualLayer(alternatingLayer);
            AssignMeasuresOnNextLayer(_layers[nextLayerIndex], straightSweep, cancellationToken);
        }

        private AlternatingLayer InitialOrderingOfNextLayer(AlternatingLayer alternatingLayer, IList<SugiVertex> nextLayer, bool straightSweep)
        {
            //get the list of the containers and vertices
            Stack<ISegmentContainer> segmentContainerStack = new Stack<ISegmentContainer>(alternatingLayer.OfType<ISegmentContainer>().Reverse());
            VertexTypes ignorableVertexType = straightSweep ? VertexTypes.QVertex : VertexTypes.PVertex;
            Stack<SugiVertex> vertexStack = new Stack<SugiVertex>(nextLayer.Where(v => v.Type != ignorableVertexType).OrderBy(v => v.MeasuredPosition).Reverse());
            AlternatingLayer newAlternatingLayer = new AlternatingLayer();

            while (vertexStack.Count > 0 && segmentContainerStack.Count > 0)
            {
                SugiVertex vertex = vertexStack.Peek();
                ISegmentContainer segmentContainer = segmentContainerStack.Peek();
                if (vertex.MeasuredPosition <= segmentContainer.Position)
                {
                    newAlternatingLayer.Add(vertexStack.Pop());
                }
                else if (vertex.MeasuredPosition >= (segmentContainer.Position + segmentContainer.Count - 1))
                {
                    newAlternatingLayer.Add(segmentContainerStack.Pop());
                }
                else
                {
                    vertexStack.Pop();
                    segmentContainerStack.Pop();
                    int k = (int)Math.Ceiling(vertex.MeasuredPosition - segmentContainer.Position);
                    segmentContainer.Split(k, out ISegmentContainer sc1, out ISegmentContainer sc2);
                    newAlternatingLayer.Add(sc1);
                    newAlternatingLayer.Add(vertex);
                    sc2.Position = segmentContainer.Position + k;
                    segmentContainerStack.Push(sc2);
                }
            }
            if (vertexStack.Count > 0)
            {
                newAlternatingLayer.AddRange(vertexStack.OfType<IData>());
            }

            if (segmentContainerStack.Count > 0)
            {
                newAlternatingLayer.AddRange(segmentContainerStack.OfType<IData>());
            }

            return newAlternatingLayer;
        }

        /// <summary>
        /// Assigns the positions of the vertices and segment container
        /// on the actual layer.
        /// </summary>
        /// <param name="alternatingLayer">The actual layer (L_i).</param>
        private static void AssignPositionsOnActualLayer(AlternatingLayer alternatingLayer)
        {
            //assign positions to vertices on the actualLayer (L_i)
            for (int i = 1; i < alternatingLayer.Count; i += 2)
            {
                SegmentContainer precedingContainer = alternatingLayer[i - 1] as SegmentContainer;
                SugiVertex vertex = alternatingLayer[i] as SugiVertex;
                if (i == 1)
                {
                    vertex.Position = precedingContainer.Count;
                }
                else
                {
                    SugiVertex previousVertex = alternatingLayer[i - 2] as SugiVertex;
                    vertex.Position = previousVertex.Position + precedingContainer.Count + 1;
                }
            }

            //assign positions to containers on the actualLayer (L_i+1)
            for (int i = 0; i < alternatingLayer.Count; i += 2)
            {
                SegmentContainer container = alternatingLayer[i] as SegmentContainer;
                if (i == 0)
                {
                    container.Position = 0;
                }
                else
                {
                    SugiVertex precedingVertex = alternatingLayer[i - 1] as SugiVertex;
                    container.Position = precedingVertex.Position + 1;
                }
            }
        }

        private void AssignMeasuresOnNextLayer(IList<SugiVertex> layer, bool straightSweep, CancellationToken cancellationToken)
        {
            //measures of the containers is the same as their positions
            //so we should set the measures only for the vertices
            foreach (SugiVertex vertex in layer)
            {
                if ((straightSweep && vertex.Type == VertexTypes.QVertex)
                    || (!straightSweep && vertex.Type == VertexTypes.PVertex))
                {
                    continue;
                }

                IEnumerable<SugiEdge> edges = straightSweep ? _graph.InEdges(vertex) : _graph.OutEdges(vertex);
                double oldMeasuredPosition = vertex.MeasuredPosition;
                vertex.MeasuredPosition = 0;
                vertex.DoNotOpt = false;
                int count = 0;
                foreach (SugiEdge edge in edges)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    SugiVertex otherVertex = edge.OtherVertex(vertex);
                    vertex.MeasuredPosition += otherVertex.Position;
                    count++;
                }
                if (count > 0)
                {
                    vertex.MeasuredPosition /= count;
                }
                else
                {
                    vertex.MeasuredPosition = oldMeasuredPosition;
                    //vertex.MeasuredPosition = vertex.Position;
                    vertex.DoNotOpt = true;
                }
            }
        }
    }
}