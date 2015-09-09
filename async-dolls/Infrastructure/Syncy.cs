using System.Threading;

namespace AsyncDolls
{
    class Syncy : SynchronizationContext
    {
        private int counter;
        private readonly string _name;

        public Syncy(string name)
        {
            _name = name;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new Syncy(_name);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            d(state);
        }

        public override string ToString()
        {
            return counter++ + " " + _name;
        }
    }
}