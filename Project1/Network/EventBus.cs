using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Net
{
    public class EventBus
    {
        private class Subscription
        {
            public object Owner;
            public Delegate OriginalHandler;
            public Action<GameEvent> WrappedHandler;
        }

        readonly Dictionary<int, List<Subscription>> _eventBus = [];
        public void Post<T>(T args) where T : EventPayloadBase
        {
            if (Registry.GameEvents.TryGet<T>(out var id))
            {
                var e = new GameEvent(id, args);
                this.Post(e);
            }
            //else
            //    throw new Exception();
        }
        public void Post<T>(TimeSpan time, T args) where T : EventPayloadBase
        {
            if (Registry.GameEvents.TryGet<T>(out var id))
            {
                var e = new GameEvent(time.TotalMilliseconds, id, args);
                this.Post(e);
            }
        }
        protected virtual void Post(GameEvent a)
        {
            var id = a.Type;
            if (_eventBus.TryGetValue(id, out var list))
                foreach (var i in list)
                    i.WrappedHandler(a);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="handler"></param>
        /// <returns>An unsubscribe callback. Call it to unregister the listener</returns>
        public Action ListenTo<TPayload>(Action<TPayload> handler) where TPayload : EventPayloadBase
        {
            var id = Registry.GameEvents.Register<TPayload>();
            if (!_eventBus.TryGetValue(id, out var list))
                _eventBus[id] = list = [];
            var wrapped = new Action<GameEvent>(e => handler((TPayload)e.Payload));
            list.Add(new Subscription { OriginalHandler = handler, WrappedHandler = wrapped, Owner = handler.Target });

            return () => _unsubscribe(handler);
        }
        public Action ListenTo(Type payloadType, Action<EventPayloadBase> handler)
        {
            var id = Registry.GameEvents.Register(payloadType);
            if (!_eventBus.TryGetValue(id, out var list))
                _eventBus[id] = list = [];
            var wrapped = new Action<GameEvent>(e => handler(e.Payload));
            list.Add(new Subscription { OriginalHandler = handler, WrappedHandler = wrapped, Owner = handler.Target });

            return () => _unsubscribe(handler);
        }
        void _unsubscribe<TPayload>(Action<TPayload> handler) where TPayload : EventPayloadBase
        {
            var id = Registry.GameEvents.Register<TPayload>();
            if (_eventBus.TryGetValue(id, out var list))
            {
                var sub = list.FirstOrDefault(s => s.OriginalHandler == (Delegate)handler);
                if (sub != null) list.Remove(sub);
            }
        }
        public void Unsubscribe(object owner)
        {
            foreach (var list in _eventBus.Values)
                list.RemoveAll(s => s.Owner == owner);
        }
    }
    //public class EventBus
    //{

    //    readonly Dictionary<int, List<Action<GameEvent>>> _eventBus = [];
    //    public void Post<T>(T args) where T : EventPayloadBase
    //    {
    //        if (Registry.GameEvents.TryGet<T>(out var id))
    //        {
    //            var e = new GameEvent(id, args);
    //            this.Post(e);
    //        }
    //        //else
    //        //    throw new Exception();
    //    }
    //    public void Post<T>(TimeSpan time, T args) where T : EventPayloadBase
    //    {
    //        if (Registry.GameEvents.TryGet<T>(out var id))
    //        {
    //            var e = new GameEvent(time.TotalMilliseconds, id, args);
    //            this.Post(e);
    //        }
    //    }
    //    protected virtual void Post(GameEvent a)
    //    {
    //        var id = a.Type;
    //        if (_eventBus.TryGetValue(id, out var list))
    //            foreach (var i in list)
    //                i(a);
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <typeparam name="TPayload"></typeparam>
    //    /// <param name="handler"></param>
    //    /// <returns>An unsubscribe callback. Call it to unregister the listener</returns>
    //    public Action ListenTo<TPayload>(Action<TPayload> handler) where TPayload : EventPayloadBase
    //    {
    //        var id = Registry.GameEvents.Register<TPayload>();
    //        if (!_eventBus.TryGetValue(id, out var list))
    //        {
    //            list = new List<Action<GameEvent>>();
    //            _eventBus[id] = list;
    //        }
    //        var item = new Action<GameEvent>(e => handler((TPayload)e.Payload));
    //        list.Add(item);
    //        return () => _stopListening<TPayload>(item);
    //    }
    //    public void _stopListening<TPayload>(Action<GameEvent> handler) where TPayload : EventPayloadBase
    //    {
    //        var id = Registry.GameEvents.Register<TPayload>();
    //        if (!_eventBus.TryGetValue(id, out var list))
    //        {
    //            throw new Exception();
    //        }
    //        list.Remove(handler);
    //    }
    //}
}
