using Project1.Core.Net;
using Project1.Framework.Events;

namespace Project1.Core.Base
{
    public class Registry
    {
        public static readonly PacketRegistry PacketHandlers = new();
        public static readonly EventRegistry GameEvents = new();
        public static readonly EventHooks MapEventHooksServer = new();
        public static readonly EventHooks MapEventHooksClient = new();
        public static readonly EventHooks WorldEventHooksServer = new();
        public static readonly EventHooks PlayerInputEventHooks = new();
    }
}
