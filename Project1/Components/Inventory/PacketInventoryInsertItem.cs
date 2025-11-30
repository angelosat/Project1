using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketInventoryInsertItem
    {
        static readonly int p;
        static PacketInventoryInsertItem()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetPeer net, Actor actor, Entity item, OffsiteAreaDef area)
        {
            if (net is not Server server)
                throw new Exception();

            var stream = server.OutgoingStreamTimestamped;
            stream.Write(p);
            stream.Write(actor.RefID);
            stream.Write(item.RefID);
            stream.Write(area.Name);
        }
        static public void Receive(INetPeer net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();

            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.GetNetworkEntity(itemID) as Entity;
            var actor = net.GetNetworkEntity(actorID) as Actor;
            var area = Def.GetDef<OffsiteAreaDef>(r.ReadString());
            actor.Loot(item, area);
        }
    }
}
