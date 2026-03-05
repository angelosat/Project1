using System;

namespace Project1.Framework.Events
{
    //public interface IObservable
    //{
    //    ChangeNotifier Notifications { get; }
    //}
    //public class Notifications
    //{
    //    event Action Updated;
    //    public void NotifyUpdated() => this.Updated?.Invoke();
    //    public IDisposable Subscribe(Action handler)
    //    {
    //        this.Updated += handler;
    //        return new Subscription(() => remove(handler));
    //        void remove(Action handler)
    //        {
    //            this.Updated -= handler;
    //        }
    //    }
    //}
    public class ChangeNotifier
    {
        /*public*/ event Action Updated;
        internal void Notify() => this.Updated?.Invoke();
        internal IDisposable Subscribe(Action handler)
        {
            this.Updated += handler;
            return new Subscription(() => remove(handler));
            void remove(Action handler)
            {
                this.Updated -= handler;
            }
        }
    }
}
