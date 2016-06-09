using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsPartial
{
    public class Chain
    {
        readonly List<ElementInstance> executingElements;
        private Stack<Tuple<Context, ElementInstance>> afterElements;

        public Chain(IEnumerable<ILinkElement> elements)
        {
            executingElements = elements.Select(s => new ElementInstance(s)).ToList();
            afterElements = new Stack<Tuple<Context, ElementInstance>>();
        }

        public Task Invoke(Context context)
        {
            return InnerInvoke(context, new Index());
        }

        async Task InnerInvoke(Context context, Index index)
        {
            ElementInstance element;
            for (int i = index.Value; i < executingElements.Count; i++)
            {
                index.Value = i;
                element = executingElements[index.Value];
                if (element.IsBefore)
                {
                    await element.Invoke(context, ctx => Task.CompletedTask).ConfigureAwait(false);
                    continue;
                }

                if (element.IsSurround)
                {
                    index.Value += 1;
                    await element.Invoke(context, ctx => InnerInvoke(ctx, index)).ConfigureAwait(false);
                    i = index.Value++;
                    continue;
                }

                if (element.IsAfter)
                {
                    afterElements.Push(Tuple.Create(context, element));
                }
            }

            if (index.Value == executingElements.Count)
            {
                foreach (var contextAndElement in afterElements)
                {
                    await contextAndElement.Item2.Invoke(contextAndElement.Item1, ctx => Task.CompletedTask).ConfigureAwait(false);
                }
            }
        }

        class Index
        {
            public int Value;
        }
    }
}