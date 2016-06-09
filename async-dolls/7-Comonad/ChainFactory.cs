using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.Comonaden
{
    public class ChainFactory
    {
        private readonly Queue<Func<ILinkElement>> registeredElementFactories = new Queue<Func<ILinkElement>>();

        public ChainFactory Register(Func<ILinkElement> elementFactory)
        {
            registeredElementFactories.Enqueue(elementFactory);

            return this;
        }

        public Chain Create()
        {
            var elements = registeredElementFactories.Select(stepFactory => stepFactory()).ToList();

            return new Chain(elements);
        }
    }
}