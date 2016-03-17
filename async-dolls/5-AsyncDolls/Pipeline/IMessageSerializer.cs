using System;
using System.IO;

namespace AsyncDolls.Pipeline
{
    public interface IMessageSerializer
    {
        string ContentType { get; }
        void Serialize(object message, Stream body);
        object Deserialize(Stream body, Type messageType);
    }
}