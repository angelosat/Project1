using System;
using System.Collections.Generic;

namespace Project1.Framework.Events
{
    public class EventRegistry
    {
        readonly Dictionary<Type, int> _registry = [];
        internal int Register<TPayload>() where TPayload : IEventPayload
        {
            var t = typeof(TPayload);
            if (this._registry.TryGetValue(t, out var existing))
                return existing;
            var id = this._registry.Count;
            this._registry[t] = id;
            return id;
        }
        public bool TryGet<T>(out int id) where T : IEventPayload
        {
            return _registry.TryGetValue(typeof(T), out id);
        }

        internal int Register(Type t)
        {
            if (this._registry.TryGetValue(t, out var existing))
                return existing;
            var id = this._registry.Count;
            this._registry[t] = id;
            return id;
        }
    }
}
