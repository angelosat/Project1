using System;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Core.World.WorldAreas;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Net;

namespace Project1.Core.Inventory
{
    [EnsureStaticCtorCall]
    static class PacketInventoryInsertItem
    {
        static readonly int p;
        static PacketInventoryInsertItem()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(INetEndpoint net, Actor actor, Entity item, FrontierDef area)
        {
            if (net is not Server server)
                throw new Exception();

            var stream = server.BeginPacket(p);
            stream.Write(actor.RefId);
            stream.Write(item.RefId);
            stream.Write(area.Name);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();

            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.World.GetEntity(itemID) as Entity;
            var actor = net.World.GetEntity(actorID) as Actor;
            var area = Def.GetDef<FrontierDef>(r.ReadString());
            actor.Loot(item, area);
        }
    }
}
