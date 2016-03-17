namespace AsyncDolls.Testing
{
    public static class BrokerExtensions
    {
        public static void Start(this Broker broker)
        {
            broker.StartAsync().Wait();
        }

        public static void Stop(this Broker broker)
        {
            broker.StopAsync().Wait();
        }
    }
}