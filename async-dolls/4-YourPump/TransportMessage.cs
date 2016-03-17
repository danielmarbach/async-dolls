using System;
using System.Collections.Generic;
using System.IO;
using AsyncDolls;

namespace AsyncDolls.YourPump
{
    public class TransportMessage
    {
        public TransportMessage()
        {
            var id = CombGuid.Generate().ToString();

            Headers = new Dictionary<string, string>
            {
                {HeaderKeys.MessageId, id},
            };
        }

        public TransportMessage(TransportMessage message)
        {
            Headers = new Dictionary<string, string>
            {
                {HeaderKeys.MessageId, message.Id},
            };

            foreach (var pair in message.Headers)
            {
                if (!Headers.ContainsKey(pair.Key))
                {
                    Headers.Add(pair.Key, pair.Value);
                }
            }
        }

        public string Id
        {
            get { return Headers[HeaderKeys.MessageId]; }
        }

        public Dictionary<string, string> Headers { get; }

    }
}