using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityDespawn
    {
        static readonly int _packetTypeId;
        static PacketEntityDespawn()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(Server server, Entity entity)
        {
            //if (net is Client)
            //    return;
            var w = server.BeginTimestamped(_packetTypeId);
            w.Write(entity.RefId);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var client = net as Client;
            var refid = r.ReadInt32();
            var entity = client.World.GetEntity(refid);
            var map = client.Map as StaticMap;
            map.Despawn(entity);
            //actor.OnDespawn();
            //Send(net, actor);
        }
    }
}
