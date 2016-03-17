using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AsyncDolls.Pipeline.Outgoing
{
    public static class MessageRouterExtensions
    {
        public static IReadOnlyCollection<Address> NoDestination(this IMessageRouter router)
        {
            return new ReadOnlyCollection<Address>(new List<Address>());
        }

        public static IReadOnlyCollection<Address> To(this IMessageRouter router, Queue queue)
        {
            return new ReadOnlyCollection<Address>(new List<Address>
            {
                queue
            });
        }

        public static IReadOnlyCollection<Address> To(this IMessageRouter router, Topic topic)
        {
            return new ReadOnlyCollection<Address>(new List<Address>
            {
                topic
            });
        }
    }
}