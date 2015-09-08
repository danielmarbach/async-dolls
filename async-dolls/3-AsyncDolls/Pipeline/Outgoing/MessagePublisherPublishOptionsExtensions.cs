namespace AsyncDolls.Pipeline.Outgoing
{
    public static class MessagePublisherPublishOptionsExtensions
    {
        public static string Destination(this PublishOptions options)
        {
            return options.Topic.Destination;
        }
    }
}