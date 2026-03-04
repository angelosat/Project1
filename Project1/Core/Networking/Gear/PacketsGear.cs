using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Gear
{
    [EnsureStaticCtorCall]
    static class PacketsGear
    {
        static int _packetTypeIdEquip, _packetTypeIdUnequip;
        static PacketsGear()
        {
            _packetTypeIdEquip = Registry.PacketHandlers.Register(ReceiveEquip);
            _packetTypeIdUnequip = Registry.PacketHandlers.Register(ReceiveUnequip);
        }
        static internal void SendEquip(Actor actor, Entity item)
        {
            var server = actor.Net as Server;
            server.BeginPacket(_packetTypeIdEquip)
                .Write(actor.RefId)
                .Write(item.RefId);
        }
        static internal void SendUnequip(Actor actor, GearTypeDef slot)
        {
            var server = actor.Net as Server;
            server.BeginPacket(_packetTypeIdUnequip)
                .Write(actor.RefId)
                .Write(slot);
        }
        static void ReceiveEquip(NetEndpoint net, Packet packet)
        {
            var client = net as Client;
            var r = packet.PacketReader;
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            var item = net.World.GetEntity(r.ReadInt32());
            actor.Gear.Equip(item);
        }
        static void ReceiveUnequip(NetEndpoint net, Packet packet)
        {
            var client = net as Client;
            var r = packet.PacketReader;
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            var slot = r.ReadDef<GearTypeDef>();
            actor.Gear.Unequip(slot);
        }
    }
}
