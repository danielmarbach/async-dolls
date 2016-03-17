using System;
using System.IO;
using Newtonsoft.Json;

namespace AsyncDolls.Pipeline
{
    public class NewtonsoftJsonMessageSerializer : IMessageSerializer
    {
        public string ContentType
        {
            get { return "application/json"; }
        }

        public void Serialize(object message, Stream body)
        {
            var streamWriter = new StreamWriter(body);
            var writer = new JsonTextWriter(streamWriter);
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, message);
            streamWriter.Flush();

            body.Flush();
            body.Position = 0;
        }

        public object Deserialize(Stream body, Type messageType)
        {
            var streamReader = new StreamReader(body);
            var reader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize(reader, messageType);
        }
    }
}