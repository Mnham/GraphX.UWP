﻿using QuikGraph;

using System.Collections.Generic;
using System.Linq;

namespace GraphX.Logic.Algorithms
{
    public class HierarchicalGraph<TVertex, TEdge> :
        BidirectionalGraph<TVertex, TEdge>, IHierarchicalBidirectionalGraph<TVertex, TEdge>
        where TEdge : TypedEdge<TVertex>
    {
        #region Properties, fields

        private class TypedEdgeCollectionWrapper
        {
            public readonly List<TEdge> InHierarchicalEdges = new List<TEdge>();
            public readonly List<TEdge> OutHierarchicalEdges = new List<TEdge>();
            public readonly List<TEdge> InGeneralEdges = new List<TEdge>();
            public readonly List<TEdge> OutGeneralEdges = new List<TEdge>();
        }

        private readonly Dictionary<TVertex, TypedEdgeCollectionWrapper> _typedEdgeCollections = new Dictionary<TVertex, TypedEdgeCollectionWrapper>();

        #endregion Properties, fields

        #region Constructors

        public HierarchicalGraph()
        { }

        public HierarchicalGraph(bool allowParallelEdges)
            : base(allowParallelEdges) { }

        public HierarchicalGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity) { }

        #endregion Constructors

        #region Add/Remove Vertex

        public override bool AddVertex(TVertex v)
        {
            base.AddVertex(v);
            if (!_typedEdgeCollections.ContainsKey(v))
            {
                _typedEdgeCollections[v] = new TypedEdgeCollectionWrapper();
            }
            return true;
        }

        public override bool RemoveVertex(TVertex v)
        {
            bool ret = base.RemoveVertex(v);
            if (ret)
            {
                //remove the edges from the typedEdgeCollections
                TypedEdgeCollectionWrapper edgeCollection = _typedEdgeCollections[v];
                foreach (TEdge e in edgeCollection.InGeneralEdges)
                {
                    _typedEdgeCollections[e.Source].OutGeneralEdges.Remove(e);
                }

                foreach (TEdge e in edgeCollection.OutGeneralEdges)
                {
                    _typedEdgeCollections[e.Target].InGeneralEdges.Remove(e);
                }

                foreach (TEdge e in edgeCollection.InHierarchicalEdges)
                {
                    _typedEdgeCollections[e.Source].OutHierarchicalEdges.Remove(e);
                }

                foreach (TEdge e in edgeCollection.OutHierarchicalEdges)
                {
                    _typedEdgeCollections[e.Target].InHierarchicalEdges.Remove(e);
                }

                _typedEdgeCollections.Remove(v);
                return true;
            }

            return false;
        }

        #endregion Add/Remove Vertex

        #region Add/Remove Edge

        public override bool AddEdge(TEdge e)
        {
            if (!base.AddEdge(e))
            {
                return false;
            }
            //add edge to the source collections
            TypedEdgeCollectionWrapper sourceEdgeCollection = _typedEdgeCollections[e.Source];
            switch (e.Type)
            {
                case EdgeTypes.General:
                    sourceEdgeCollection.OutGeneralEdges.Add(e);
                    break;

                case EdgeTypes.Hierarchical:
                    sourceEdgeCollection.OutHierarchicalEdges.Add(e);
                    break;
            }

            //add edge to the target collections
            TypedEdgeCollectionWrapper targetEdgeCollection = _typedEdgeCollections[e.Target];
            switch (e.Type)
            {
                case EdgeTypes.General:
                    targetEdgeCollection.InGeneralEdges.Add(e);
                    break;

                case EdgeTypes.Hierarchical:
                    targetEdgeCollection.InHierarchicalEdges.Add(e);
                    break;
            }
            return true;
        }

        public override bool RemoveEdge(TEdge e)
        {
            if (!base.RemoveEdge(e))
            {
                return false;
            }
            //remove edge from the source collections
            TypedEdgeCollectionWrapper sourceEdgeCollection = _typedEdgeCollections[e.Source];
            switch (e.Type)
            {
                case EdgeTypes.General:
                    sourceEdgeCollection.OutGeneralEdges.Remove(e);
                    break;

                case EdgeTypes.Hierarchical:
                    sourceEdgeCollection.OutHierarchicalEdges.Remove(e);
                    break;
            }

            //remove edge from the target collections
            TypedEdgeCollectionWrapper targetEdgeCollection = _typedEdgeCollections[e.Target];
            switch (e.Type)
            {
                case EdgeTypes.General:
                    targetEdgeCollection.InGeneralEdges.Remove(e);
                    break;

                case EdgeTypes.Hierarchical:
                    targetEdgeCollection.InHierarchicalEdges.Remove(e);
                    break;
            }
            return true;
        }

        #endregion Add/Remove Edge

        #region Hierarchical Edges

        public IEnumerable<TEdge> HierarchicalEdgesFor(TVertex v)
        {
            TypedEdgeCollectionWrapper collections = _typedEdgeCollections[v];
            return collections.InHierarchicalEdges.Concat(collections.OutHierarchicalEdges);
        }

        public int HierarchicalEdgeCountFor(TVertex v)
        {
            return _typedEdgeCollections[v].InHierarchicalEdges.Count + _typedEdgeCollections[v].OutHierarchicalEdges.Count;
        }

        public IEnumerable<TEdge> InHierarchicalEdges(TVertex v)
        {
            return _typedEdgeCollections[v].InHierarchicalEdges;
        }

        public int InHierarchicalEdgeCount(TVertex v)
        {
            return _typedEdgeCollections[v].InHierarchicalEdges.Count;
        }

        public IEnumerable<TEdge> OutHierarchicalEdges(TVertex v)
        {
            return _typedEdgeCollections[v].OutHierarchicalEdges;
        }

        public int OutHierarchicalEdgeCount(TVertex v)
        {
            return _typedEdgeCollections[v].OutHierarchicalEdges.Count;
        }

        #endregion Hierarchical Edges

        #region General Edges

        public IEnumerable<TEdge> GeneralEdgesFor(TVertex v)
        {
            TypedEdgeCollectionWrapper collections = _typedEdgeCollections[v];
            foreach (TEdge e in collections.InGeneralEdges)
            {
                yield return e;
            }
            foreach (TEdge e in collections.OutGeneralEdges)
            {
                yield return e;
            }
        }

        public int GeneralEdgeCountFor(TVertex v)
        {
            return _typedEdgeCollections[v].InGeneralEdges.Count + _typedEdgeCollections[v].OutGeneralEdges.Count;
        }

        public IEnumerable<TEdge> InGeneralEdges(TVertex v)
        {
            return _typedEdgeCollections[v].InGeneralEdges;
        }

        public int InGeneralEdgeCount(TVertex v)
        {
            return _typedEdgeCollections[v].InGeneralEdges.Count;
        }

        public IEnumerable<TEdge> OutGeneralEdges(TVertex v)
        {
            return _typedEdgeCollections[v].OutGeneralEdges;
        }

        public int OutGeneralEdgeCount(TVertex v)
        {
            return _typedEdgeCollections[v].OutGeneralEdges.Count;
        }

        #endregion General Edges

        #region IHierarchicalBidirectionalGraph<TVertex,TEdge> Members

        public IEnumerable<TEdge> HierarchicalEdges => Vertices.SelectMany(OutHierarchicalEdges);

        public int HierarchicalEdgeCount => Vertices.Sum(v => InHierarchicalEdgeCount(v));

        public IEnumerable<TEdge> GeneralEdges => Vertices.SelectMany(OutGeneralEdges);

        public int GeneralEdgeCount => Vertices.Sum(v => InGeneralEdgeCount(v));

        #endregion IHierarchicalBidirectionalGraph<TVertex,TEdge> Members
    }
}