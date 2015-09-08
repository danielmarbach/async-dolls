using System;
using System.Globalization;

namespace AsyncDolls
{
    public class Topic : Address
    {
        private const string Schema = "topic://";

        private Topic(string address)
            : base(address, Schema)
        {
            if (!address.StartsWith(Schema, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Topic name must start with {0}", Schema));
            }
        }

        public static Topic Create(string addressPossiblyWithoutSchema)
        {
            Topic queue;
            return TryParse(addressPossiblyWithoutSchema, out queue) ?
                queue : new Topic(string.Format(CultureInfo.InvariantCulture, "{0}{1}", Schema, addressPossiblyWithoutSchema));
        }

        public static bool TryParse(string address, out Topic topic)
        {
            topic = null;

            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            if (address.StartsWith(Schema, StringComparison.InvariantCultureIgnoreCase))
            {
                topic = new Topic(address);
                return true;
            }

            return false;
        }
    }
}