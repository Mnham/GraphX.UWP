namespace GraphX.Logic.Algorithms
{
    public class WrappedVertex<TVertex>
    {
        private readonly TVertex _originalVertex;
        public TVertex Original => _originalVertex;

        public WrappedVertex(TVertex original)
        {
            _originalVertex = original;
        }
    }
}