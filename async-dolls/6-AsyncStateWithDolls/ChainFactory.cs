using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.AsyncStateWithDolls
{
    public class ChainFactory
    {
        private readonly Queue<Func<ILinkElement>> registeredLinkElementFactories = new Queue<Func<ILinkElement>>();

        public ChainFactory Register(Func<ILinkElement> elementFactory)
        {
            registeredLinkElementFactories.Enqueue(elementFactory);

            return this;
        }

        public Chain Create()
        {
            var elements = registeredLinkElementFactories.Select(factory => factory()).ToList();

            return new Chain(elements);
        }
    }
}