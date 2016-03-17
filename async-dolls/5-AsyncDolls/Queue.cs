using System;
using System.Globalization;

namespace AsyncDolls
{
    public class Queue : Address
    {
        const string Schema = "queue://";

        Queue(string address)
            : base(address, Schema)
        {
            if (!address.StartsWith(Schema, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Queue name must start with {0}", Schema));
            }
        }

        public static Queue Create(string addressPossiblyWithoutSchema)
        {
            Queue queue;
            return TryParse(addressPossiblyWithoutSchema, out queue) ?
                queue : new Queue(string.Format(CultureInfo.InvariantCulture, "{0}{1}", Schema, addressPossiblyWithoutSchema));
        }

        public static bool TryParse(string address, out Queue queue)
        {
            queue = null;

            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            if (address.StartsWith(Schema, StringComparison.InvariantCultureIgnoreCase))
            {
                queue = new Queue(address);
                return true;
            }

            return false;
        }
    }
}