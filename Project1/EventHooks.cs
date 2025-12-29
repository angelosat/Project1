using SharpDX.Direct3D9;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Start_a_Town_
{
    public sealed class EventHooks
    {
        private readonly List<Action<EventBus>> _attach = [];
        private readonly HashSet<(Type, MethodInfo)> _registered = [];

        public void Register<T>(Action<T> callback) where T : EventPayloadBase
        {
            var key = (typeof(T), callback.Method);
            if (!_registered.Add(key))
                return;

            _attach.Add(bus => bus.ListenTo(callback));
        }

        public void HookTo(EventBus bus)
        {
            foreach (var attach in _attach)
                attach(bus);
        }
    }
}
