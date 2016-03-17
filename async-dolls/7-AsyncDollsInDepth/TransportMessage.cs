using System;
using System.Collections.Generic;
using System.IO;

namespace AsyncDolls.AsyncDollsInDepth
{
    public class TransportMessage
    {
        Stream body;

        public TransportMessage()
        {
            var id = CombGuid.Generate().ToString();

            Headers = new Dictionary<string, string>
            {
                {HeaderKeys.MessageId, id},
                {HeaderKeys.CorrelationId, id},
                {HeaderKeys.ContentType, null},
                {HeaderKeys.ReplyTo, null},
                {HeaderKeys.MessageType, null},
                {HeaderKeys.MessageIntent, null},
                {HeaderKeys.DeliveryCount, "0" }
            };
        }

        public TransportMessage(TransportMessage message)
        {
            Headers = new Dictionary<string, string>
            {
                {HeaderKeys.MessageId, message.Id},
                {HeaderKeys.CorrelationId, message.CorrelationId},
                {HeaderKeys.MessageType, message.MessageType},
                {HeaderKeys.ReplyTo, message.ReplyTo?.ToString() }
            };

            var stream = new MemoryStream();
            message.Body.Position = 0;
            message.Body.CopyTo(stream);
            stream.Position = 0;
            SetBody(stream);

            foreach (var pair in message.Headers)
            {
                if (!Headers.ContainsKey(pair.Key))
                {
                    Headers.Add(pair.Key, pair.Value);
                }
            }
        }

        public string Id => Headers[HeaderKeys.MessageId];

        public string CorrelationId
        {
            get { return Headers[HeaderKeys.CorrelationId]; }
            set { Headers[HeaderKeys.CorrelationId] = value; }
        }

        public string ContentType
        {
            get { return Headers[HeaderKeys.ContentType]; }
            set { Headers[HeaderKeys.ContentType] = value; }
        }

        public string MessageType
        {
            get { return Headers[HeaderKeys.MessageType]; }
            set { Headers[HeaderKeys.MessageType] = value; }
        }

        public MessageIntent MessageIntent
        {
            get
            {
                MessageIntent messageIntent;
                string messageIntentString = Headers[HeaderKeys.MessageIntent];
                Enum.TryParse(messageIntentString, true, out messageIntent);
                return messageIntent;
            }

            set { Headers[HeaderKeys.MessageIntent] = value.ToString(); }
        }

        public Queue ReplyTo
        {
            get { return (Queue)Headers[HeaderKeys.ReplyTo].Parse(); }
            set { Headers[HeaderKeys.ReplyTo] = value.ToString(); }
        }

        public virtual int DeliveryCount
        {
            get { return int.Parse(Headers[HeaderKeys.DeliveryCount]); }
            set { Headers[HeaderKeys.DeliveryCount] = value.ToString(); }
        }

        public IDictionary<string, string> Headers { get; }

        public Stream Body => body ?? (body = new MemoryStream());

        public void SetBody(Stream body)
        {
            if (this.body != null)
            {
                throw new InvalidOperationException("Body is already set.");
            }

            this.body = body;
        }
    }
}