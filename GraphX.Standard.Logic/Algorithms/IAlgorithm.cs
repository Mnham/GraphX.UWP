using System.Threading;

namespace GraphX.Logic.Algorithms
{
    /// <summary>
    /// Simple algorithm interface which is not connected to any graph.
    /// </summary>
    public interface IAlgorithm
    {
        void Compute(CancellationToken cancellationToken);
    }
}