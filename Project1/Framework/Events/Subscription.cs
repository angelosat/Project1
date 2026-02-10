using System;

namespace Project1.Framework.Events
{
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
