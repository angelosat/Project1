using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class Registry
    {
        public static readonly PacketRegistry PacketHandlers = new();
        public static readonly GameEventRegistry GameEvents = new();
        public static readonly EventHooks MapEventHooks = new();
        public static readonly EventHooks MapEventHooksClient = new();
        public static readonly EventHooks WorldEventHooks = new();
    }
}
