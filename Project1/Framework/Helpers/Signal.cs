using System;

namespace Project1.Framework.UI
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
