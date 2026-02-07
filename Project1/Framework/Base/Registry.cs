using Project1.Core.Net;

namespace Project1.Core.Base
{
    public class Registry
    {
        public static readonly PacketRegistry PacketHandlers = new();
        public static readonly GameEventRegistry GameEvents = new();
        public static readonly EventHooks MapEventHooksServer = new();
        public static readonly EventHooks MapEventHooksClient = new();
        public static readonly EventHooks WorldEventHooksServer = new();
        public static readonly EventHooks PlayerInputEventHooks = new();
    }
}
