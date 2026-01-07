using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsAttributes
    {
        readonly static PacketId _pAttributeAdjusted;
        static PacketsAttributes()
        {
            Registry.MapEventHooksServer.Register<AttributeAdjustedEvent>(SendAttributeAdjusted);
            _pAttributeAdjusted = Registry.PacketHandlers.Register(OnAttributeAdjusted);
        }
        private static void SendAttributeAdjusted(AttributeAdjustedEvent @event)
        {
            Server.Instance.BeginPacket(_pAttributeAdjusted)
                .Write(@event.Owner.RefId)
                .Write(@event.Def)
                .Write(@event.Value);
        }
        static void OnAttributeAdjusted(NetEndpoint endpoint, Packet packet)
        {
            endpoint.World
                .GetEntity<Actor>(packet.PacketReader.ReadEntityRefId())
                .Attributes.SetValue(packet.PacketReader.ReadDef<AttributeDef>(), packet.PacketReader.ReadSingle());
        }
    }
}
