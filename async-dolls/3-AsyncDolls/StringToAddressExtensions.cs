namespace AsyncDolls
{
    using Properties;

    public static class StringToAddressExtensions
    {
        [ContractAnnotation("null => null")]
        public static Address Parse(this string address)
        {
            Queue queue;
            if (Queue.TryParse(address, out queue))
            {
                return queue;
            }

            Topic topic;
            if (Topic.TryParse(address, out topic))
            {
                return topic;
            }

            return null;
        }
    }
}