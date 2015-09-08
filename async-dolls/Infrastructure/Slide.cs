using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls
{
    public class Slide : List<Func<Task>>
    {
        public Slide(string title = default(string))
        {
        }

        public Slide BulletPoint(string empty)
        {
            return this;
        }

        public Slide Sample(Func<Task> function)
        {
            this.Add(function);
            return this;
        }

        public Slide Sample(Action action)
        {
            this.Add(() =>
            {
                action();
                return Task.FromResult(true);
            });

            return this;
        }
    }
}