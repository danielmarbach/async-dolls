namespace AsyncDolls.Pipeline.Outgoing
{
    public static class MessageSenderSendOptionsExtensions
    {
        public static string Destination(this SendOptions options)
        {
            return options.Queue.Destination;
        }
    }
}