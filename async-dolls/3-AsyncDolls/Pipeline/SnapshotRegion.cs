using System;

namespace AsyncDolls.Pipeline
{
    public sealed class SnapshotRegion : IDisposable
    {
        readonly ISupportSnapshots[] chain;

        public SnapshotRegion(params ISupportSnapshots[] chain)
        {
            this.chain = chain;

            foreach (ISupportSnapshots snapshotable in this.chain)
            {
                snapshotable.TakeSnapshot();
            }
        }

        public void Dispose()
        {
            foreach (ISupportSnapshots snapshotable in chain)
            {
                snapshotable.DeleteSnapshot();
            }
        }
    }
}