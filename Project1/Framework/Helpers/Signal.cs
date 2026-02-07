using Project1.Core.UI.Primitives;
using System;

namespace Project1.Core.Helpers
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
