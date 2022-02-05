namespace GraphX.Logic.Algorithms.OverlapRemoval
{
    public enum OneWayFSAWayEnum
    {
        Horizontal,
        Vertical
    }

    public class OneWayFSAParameters : OverlapRemovalParameters
    {
        private OneWayFSAWayEnum _way;

        public OneWayFSAWayEnum Way
        {
            get => _way;
            set => SetProperty(ref _way, value);
        }
    }
}