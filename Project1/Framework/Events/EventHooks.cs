using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Project1.Framework.Events;

public sealed class EventHooks
{
    //private readonly List<Action<EventBus>> _attach = [];
    private readonly HashSet<(Type, MethodInfo)> _registered = [];

    //private readonly HashSet<object> Targets = [];
    private readonly List<Action<EventBus, List<Action>>> _attach = [];

    public void Register<T>(Action<T> callback) where T : IEventPayload
    {
        var key = (typeof(T), callback.Method);
        if (!_registered.Add(key))
            return;

        _attach.Add((bus, tokens) =>
        {
            var unsub = bus.ListenTo(callback);
            tokens.Add(unsub);
        });
    }
    public void Register(Type payloadType, Action<IEventPayload> callback)
    {
        if (payloadType == typeof(IEventPayload))
            throw new Exception();
        var key = (payloadType, callback.Method);
        if (!_registered.Add(key))
            return;

        _attach.Add((bus, tokens) =>
        {
            var unsub = bus.ListenTo(callback);
            tokens.Add(unsub);
        });
    }
    private readonly Dictionary<EventBus, List<Action>> _active = [];

    public void HookTo(EventBus bus)
    {
        Debug.Assert(!_active.ContainsKey(bus), "Already hooked to this bus");
        if (_active.ContainsKey(bus))
            return;

        var tokens = new List<Action>();

        foreach (var attach in _attach)
            attach(bus, tokens);

        _active[bus] = tokens;
    }
    public void UnHook(EventBus bus)
    {
        if (!_active.TryGetValue(bus, out var tokens))
            return;

        foreach (var unsub in tokens)
            unsub();

        _active.Remove(bus);
    }
}
//public sealed class EventHooks
//{
//    private readonly List<Action<EventBus>> _attach = [];
//    private readonly HashSet<(Type, MethodInfo)> _registered = [];

//    private readonly HashSet<object> Targets = [];

//    public void Register<T>(Action<T> callback) where T : IEventPayload
//    {
//        var key = (typeof(T), callback.Method);
//        if (!_registered.Add(key))
//            return;
//        this.Targets.Add(callback.Target);
//        _attach.Add(bus =>  bus.ListenTo(callback));
//    }

//    public void HookTo(EventBus bus)
//    {
//        foreach (var attach in _attach)
//            attach(bus);
//    }
//    public void UnHook(EventBus bus)
//    {
//        foreach (var t in this.Targets)
//            bus.Unsubscribe(t);
//    }
//}
