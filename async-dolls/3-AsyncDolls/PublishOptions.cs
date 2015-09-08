using System;

namespace AsyncDolls
{
    public class PublishOptions : DeliveryOptions
    {
        public Type EventType { get; internal set; }
        public Topic Topic { get; internal set; }
    }
}