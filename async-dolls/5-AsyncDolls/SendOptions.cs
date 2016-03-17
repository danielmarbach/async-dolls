namespace AsyncDolls
{
    public class SendOptions : DeliveryOptions
    {
        public Queue Queue { get; set; }
        public string CorrelationId { get; set; }
    }
}