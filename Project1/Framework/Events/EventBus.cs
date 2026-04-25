using Project1.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Framework.Events;

public sealed class EventBus
{
    private sealed class Subscription
    {
        public object Owner;
        public Delegate OriginalHandler;
        public Action<GameEvent> WrappedHandler;
    }

    readonly Dictionary<int, List<Subscription>> _eventBus = [];
    public void Post<T>(T args) where T : IEventPayload
    {
        if (Registry.GameEvents.TryGet<T>(out var id))
        {
            var e = new GameEvent(id, args);
            this.Post(e);
        }
    }
    void Post(GameEvent a)
    {
        var id = a.Type;
        if (_eventBus.TryGetValue(id, out var list))
            foreach (var i in list.ToList())
                i.WrappedHandler(a);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    /// <param name="handler"></param>
    /// <returns>Returns an unsubscribe token.</returns>
    public Action ListenTo<TPayload>(Action<TPayload> handler) where TPayload : IEventPayload
    {
        if (typeof(TPayload) == typeof(IEventPayload))
            throw new Exception();
        var id = Registry.GameEvents.Register<TPayload>();
        if (!_eventBus.TryGetValue(id, out var list))
            _eventBus[id] = list = [];
        var wrapped = new Action<GameEvent>(e => handler((TPayload)e.Payload));
        list.Add(new Subscription { OriginalHandler = handler, WrappedHandler = wrapped, Owner = handler.Target });

        return () => _unsubscribe(handler);
    }
    public Action ListenTo(Type payloadType, Action<IEventPayload> handler)
    {
        var id = Registry.GameEvents.Register(payloadType);
        if (!_eventBus.TryGetValue(id, out var list))
            _eventBus[id] = list = [];
        var wrapped = new Action<GameEvent>(e => handler(e.Payload));
        list.Add(new Subscription { OriginalHandler = handler, WrappedHandler = wrapped, Owner = handler.Target });

        return () => _unsubscribe(handler);
    }
    void _unsubscribe<TPayload>(Action<TPayload> handler) where TPayload : IEventPayload
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
