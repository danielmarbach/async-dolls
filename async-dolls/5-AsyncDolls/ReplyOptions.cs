namespace AsyncDolls
{
    public class ReplyOptions : SendOptions
    {
        public ReplyOptions()
        {
        }

        public ReplyOptions(Queue destination, string correlationId)
        {
            CorrelationId = correlationId;
            Queue = destination;
        }
    }
}