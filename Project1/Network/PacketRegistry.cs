using System;
using System.Collections.Generic;

namespace Start_a_Town_.Net
{
    public class PacketRegistry
    {
        int _nextId = 40000;

        readonly Dictionary<int, Action<NetEndpoint, Packet>> _handlers = [];
        internal int Register(Action<NetEndpoint, Packet> handler)
        {
            var id = _nextId++;
            _handlers.Add(id, handler);
            return id;
        }
        public bool TryGet(int id, out Action<NetEndpoint, Packet> handler)
            => _handlers.TryGetValue(id, out handler);
    }
}
