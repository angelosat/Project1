using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsAttributes
    {
        readonly static PacketId _pAttributeIncreased;
        static PacketsAttributes()
        {
            Registry.MapEventHooks.Register<AttributeIncreasedEvent>(SendAttributeIncreased);
            _pAttributeIncreased = Registry.PacketHandlers.Register(OnAttributeIncreased);
        }
        private static void SendAttributeIncreased(AttributeIncreasedEvent @event)
        {
            Server.Instance.BeginPacket(_pAttributeIncreased)
                .Write(@event.Owner.RefId)
                .Write(@event.Def)
                .Write(@event.Delta);
        }
        static void OnAttributeIncreased(NetEndpoint endpoint, Packet packet)
        {
            endpoint.World
                .GetEntity<Actor>(packet.PacketReader.ReadEntityRefId())
                .Attributes.Adjust(packet.PacketReader.ReadDef<AttributeDef>(), packet.PacketReader.ReadSingle());
        }
    }

    [EnsureStaticCtorCall]
    internal static class PacketsResources
    {
        readonly static PacketId _pResourceAdjusted;
        static PacketsResources()
        {
            Registry.MapEventHooks.Register<ResourceAdjustedEvent>(SendResourceAdjusted);
            _pResourceAdjusted = Registry.PacketHandlers.Register(OnResourceAdjusted);
        }
        private static void SendResourceAdjusted(ResourceAdjustedEvent @event)
        {
            Server.Instance.BeginPacket(_pResourceAdjusted)
                .Write(@event.Owner.RefId)
                .Write(@event.Def)
                .Write(@event.Delta);
        }
        static void OnResourceAdjusted(NetEndpoint endpoint, Packet packet)
        {
            endpoint.World
                .GetEntity(packet.PacketReader.ReadEntityRefId())
                .Resources.Adjust(packet.PacketReader.ReadDef<ResourceDef>(), packet.PacketReader.ReadSingle());
        }
    }
}
