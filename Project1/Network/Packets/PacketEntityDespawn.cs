using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityDespawn
    {
        static readonly int _packetTypeId;
        static PacketEntityDespawn()
        {
            _packetTypeId = PacketRegistry.Register(Receive);
        }
        static public void Send(INetEndpoint net, Entity entity)
        {
            if (net is Client)
                return;
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, _packetTypeId);
            w.Write(entity.RefId);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var client = net as Client;
            var actor = client.World.GetEntity(r.ReadInt32()) as Actor;
            var map = client.Map as StaticMap;
            map.Despawn(actor);
        }
    }
}
