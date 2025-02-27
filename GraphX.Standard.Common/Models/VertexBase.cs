﻿using GraphX.Common.Enums;
using GraphX.Common.Interfaces;

namespace GraphX.Common.Models
{
    public abstract class VertexBase : ObservableObject, IGraphXVertex
    {
        /// <summary>
        /// Gets or sets custom angle associated with the vertex
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// Gets or sets optional group identificator
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Skip vertex in algo calc and visualization
        /// </summary>
        public ProcessingOptionEnum SkipProcessing { get; set; }

        protected VertexBase()
        {
            ID = -1;
        }

        /// <summary>
        /// Unique vertex ID
        /// </summary>
        public long ID { get; set; }

        public bool Equals(IGraphXVertex other)
        {
            return Equals(this, other);
        }
    }
}