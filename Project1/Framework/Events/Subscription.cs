using System;

namespace Project1.Framework.Events
{
    public class Observable
    {
        public event Action Updated;
        public void NotifyUpdated() => this.Updated?.Invoke();
        public IDisposable Subscribe(Action handler)
        {
            this.Updated += handler;
            return new Subscription(() => remove(handler));
            void remove(Action handler)
            {
                this.Updated -= handler;
            }
        }
    }
    public class Subscription(Action unsubscribe) : IDisposable
    {
        Action unsub = unsubscribe;
        bool disposed;

        public void Dispose()
        {
            if (disposed) return;
            unsub?.Invoke();
            disposed = true;
            unsub = null;
        }
    }
}
