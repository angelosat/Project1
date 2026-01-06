using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsAttributes
    {
        readonly static PacketId _pAttributeIncreased;
        static PacketsAttributes()
        {
            Registry.MapEventHooksServer.Register<AttributeIncreasedEvent>(SendAttributeIncreased);
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
}
