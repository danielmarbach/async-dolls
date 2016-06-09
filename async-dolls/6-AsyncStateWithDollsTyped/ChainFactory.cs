using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public class ChainFactory
    {
        private readonly Queue<Func<ILinkElement>> registeredElementFactory = new Queue<Func<ILinkElement>>();

        public ChainFactory Register(Func<ILinkElement> elementFactory)
        {
            registeredElementFactory.Enqueue(elementFactory);

            return this;
        }

        public Chain Create()
        {
            var elements = registeredElementFactory.Select(factory => factory()).ToList();

            return new Chain(elements);
        }
    }
}