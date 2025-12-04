using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketInventoryInsertItem
    {
        static readonly int p;
        static PacketInventoryInsertItem()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(INetEndpoint net, Actor actor, Entity item, OffsiteAreaDef area)
        {
            if (net is not Server server)
                throw new Exception();

            var stream = server.OutgoingStreamTimestamped;
            stream.Write(p);
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
            var area = Def.GetDef<OffsiteAreaDef>(r.ReadString());
            actor.Loot(item, area);
        }
    }
}
