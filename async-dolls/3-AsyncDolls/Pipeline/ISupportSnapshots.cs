namespace AsyncDolls.Pipeline
{
    public interface ISupportSnapshots
    {
        void TakeSnapshot();
        void DeleteSnapshot();
    }
}