using System;

namespace AsyncDolls
{
    using Properties;

    public class EndpointConfiguration
    {
        public EndpointConfiguration()
        {
            Concurrency(Environment.ProcessorCount);
        }

        public Queue EndpointQueue { get; private set; }
        internal int MaxConcurrency { get; private set; }
        internal int PrefetchCount { get; private set; }

        public EndpointConfiguration Endpoint([NotNull] string endpointName)
        {
            EndpointQueue = Queue.Create(endpointName);
            return this;
        }

        public EndpointConfiguration Concurrency(int maxConcurrency)
        {
            MaxConcurrency = maxConcurrency;
            PrefetchCount = maxConcurrency;
            return this;
        }

        internal ReadOnly Validate()
        {
            if (EndpointQueue == null)
            {
                throw new InvalidOperationException("The endpoint name must be set by calling configuration.Endpoint(\"EndpointName\").");
            }

            return new ReadOnly(this);
        }

        public class ReadOnly
        {
            protected internal ReadOnly(EndpointConfiguration configuration)
            {
                EndpointQueue = configuration.EndpointQueue;
                MaxConcurrency = configuration.MaxConcurrency;
                PrefetchCount = configuration.PrefetchCount;
            }

            public Queue EndpointQueue { get; private set; }
            public int MaxConcurrency { get; private set; }
            public int PrefetchCount { get; private set; }
        }
    }
}