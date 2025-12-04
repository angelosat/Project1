using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityJump
    {
        static readonly int _packetTypeId;
        static PacketEntityJump()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        
        internal static void Send(INetEndpoint net, int entityID)
        {
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(_packetTypeId);
            w.Write(entityID);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var id = r.ReadInt32();
            var entity = net.World.GetEntity(id) as Actor;
            entity.Jump();
            if (net is Server)
                Send(net, entity.RefId);
        }
    }
}
