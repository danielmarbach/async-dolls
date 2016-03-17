namespace AsyncDolls.Dequeuing
{
    static class MessageReceiverEndpointConfigurationReadonlyExtensions
    {
        public static string DestinationQueue(this EndpointConfiguration.ReadOnly configuration)
        {
            return configuration.EndpointQueue.Destination;
        }
    }
}