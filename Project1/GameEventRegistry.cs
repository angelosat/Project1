using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class GameEventRegistry
    {
        readonly Dictionary<Type, int> _registry = [];
        internal int Register<TPayload>() where TPayload : EventPayloadBase
        {
            var id = this._registry.Count;
            this._registry[typeof(TPayload)] = id;
            return id;
        }
        public bool TryGet<T>(out int id) where T : EventPayloadBase
        {
            if (!_registry.TryGetValue(typeof(T), out id))
                throw new Exception();
            return true;
        }
    }
}
