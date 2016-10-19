using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.Expressions
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
            var elements = registeredElementFactories.Select(factory => factory()).ToArray();

            return new Chain(elements);
        }
    }
}