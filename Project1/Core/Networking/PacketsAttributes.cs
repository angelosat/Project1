using Project1.Framework;
using Project1.Core.Attributes;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Helpers.Structs;
using Project1.Core.Net;

namespace Project1.Core.Networking
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
