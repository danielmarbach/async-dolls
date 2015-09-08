using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.Pipeline
{
    /// <summary>
    ///     The context is a key value bag which allows typed retrieval of values.
    /// </summary>
    public abstract class Context : ISupportSnapshots
    {
        readonly Stack<IDictionary<string, Entry>> snapshots = new Stack<IDictionary<string, Entry>>();
        readonly IDictionary<string, Entry> stash = new Dictionary<string, Entry>();
        ISupportSnapshots chain;

        protected Context(EndpointConfiguration.ReadOnly configuration)
        {
            Set(configuration);
        }

        public EndpointConfiguration.ReadOnly Configuration
        {
            get { return Get<EndpointConfiguration.ReadOnly>(); }
        }

        public void TakeSnapshot()
        {
            snapshots.Push(stash.Where(x => x.Value.CandidateForSnapshot == ShouldBeSnapshotted.Yes).ToDictionary(k => k.Key, v => v.Value));
        }

        public void DeleteSnapshot()
        {
            IDictionary<string, Entry> allSnapshottedCandidates = snapshots.Pop();

            foreach (var allSnapshottedCandidate in allSnapshottedCandidates)
            {
                stash[allSnapshottedCandidate.Key] = allSnapshottedCandidate.Value;
            }
        }

        public T Get<T>()
        {
            return Get<T>(typeof(T).FullName);
        }

        public T Get<T>(string key)
        {
            Entry result;

            if (!stash.TryGetValue(key, out result))
            {
                throw new KeyNotFoundException("No item found in behavior context with key: " + key);
            }

            return (T) result.Value;
        }

        public void Set<T>(T t, ShouldBeSnapshotted candidateForSnapshot = ShouldBeSnapshotted.No)
        {
            Set(typeof(T).FullName, t, candidateForSnapshot);
        }

        public void Set<T>(string key, T t, ShouldBeSnapshotted candidateForSnapshot = ShouldBeSnapshotted.No)
        {
            stash[key] = new Entry(t, candidateForSnapshot);
        }

        internal void SetChain(ISupportSnapshots chain)
        {
            this.chain = chain;
        }

        internal IDisposable CreateSnapshot()
        {
            return new SnapshotRegion(chain, this);
        }

        class Entry
        {
            public Entry(object value, ShouldBeSnapshotted candidateForSnapshot = ShouldBeSnapshotted.No)
            {
                CandidateForSnapshot = candidateForSnapshot;
                Value = value;
            }

            public ShouldBeSnapshotted CandidateForSnapshot { get; private set; }
            public object Value { get; private set; }
        }
    }
}