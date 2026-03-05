using System;

namespace Project1.Framework.Events
{
    public class Subscription(Action unsubscribe) : IDisposable
    {
        Action unsub = unsubscribe;

        public void Dispose()
        {
            unsub?.Invoke();
            unsub = null;
        }
    }
}
