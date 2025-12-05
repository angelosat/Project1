using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class GameEventRegistry
    {
        readonly Dictionary<Type, int> _registry = [];
        internal int Register<TPayload>() where TPayload : EventPayloadBase
        {
            var t = typeof(TPayload);
            if (this._registry.TryGetValue(t, out var existing))
                return existing;
            var id = this._registry.Count;
            this._registry[t] = id;
            return id;
        }
        public bool TryGet<T>(out int id) where T : EventPayloadBase
        {
            return _registry.TryGetValue(typeof(T), out id);
            var t = typeof(T);
            if (!_registry.TryGetValue(t, out id))
                $"event {t} not registered".ToConsole();
            return true;
        }

        internal int Register(Type t)
        {
            //var t = typeof(TPayload);
            if (this._registry.TryGetValue(t, out var existing))
                return existing;
            var id = this._registry.Count;
            this._registry[t] = id;
            return id;
        }
    }
}
