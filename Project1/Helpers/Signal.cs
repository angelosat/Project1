using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public sealed class Signal
    {
        private event Action Handlers;

        public IDisposable Subscribe(Action handler)
        {
            Handlers += handler;
            return new Subscription(() => this.Handlers -= handler);
        }

        public void Raise()
        {
            this.Handlers?.Invoke();
        }
    }
}
