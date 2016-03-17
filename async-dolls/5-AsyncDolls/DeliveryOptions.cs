using System.Collections.Generic;

namespace AsyncDolls
{
    public abstract class DeliveryOptions
    {
        protected DeliveryOptions()
        {
            Headers = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Headers { get; private set; }
        public Queue ReplyToAddress { get; set; }
    }
}