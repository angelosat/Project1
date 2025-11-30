using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Core;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityDespawn
    {
        static readonly int p;
        static PacketEntityDespawn()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetPeer net, Entity entity)
        {
            if (net is Client)
                return;
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(entity.RefID);
        }
        static public void Receive(INetPeer net, BinaryReader r)
        {
            var client = net as Client;
            var actor = client.GetNetworkEntity(r.ReadInt32()) as Actor;
            var map = client.Map as StaticMap;
            map.Despawn(actor);
        }
    }
}
