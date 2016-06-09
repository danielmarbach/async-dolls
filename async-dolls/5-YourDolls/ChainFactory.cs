using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.YourDolls
{
    public class ChainFactory
    {
        private readonly Queue<Func<ILinkElement>> registeredLinkElementFactories = new Queue<Func<ILinkElement>>();

        public ChainFactory Register(Func<ILinkElement> linkElementFactory)
        {
            registeredLinkElementFactories.Enqueue(linkElementFactory);

            return this;
        }

        public Chain Create()
        {
            var steps = registeredLinkElementFactories.Select(factory => factory()).ToList();

            return new Chain(steps);
        }
    }
}