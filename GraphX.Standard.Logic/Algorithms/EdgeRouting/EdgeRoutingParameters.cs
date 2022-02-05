using GraphX.Common.Interfaces;

using System.ComponentModel;

namespace GraphX.Logic.Algorithms.EdgeRouting
{
    /// <summary>
    /// Base class for edge routing parameters
    /// </summary>
    public class EdgeRoutingParameters : IEdgeRoutingParameters
    {
        /// <summary>
        /// Clone parameters
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Calls OnPropertyChange event notification
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// PropertyChange event notification
        /// </summary>
        protected event PropertyChangedEventHandler PropertyChanged;
    }
}