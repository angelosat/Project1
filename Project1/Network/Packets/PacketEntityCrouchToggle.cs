using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityCrouchToggle
    {
        static readonly int PType;
        static PacketEntityCrouchToggle()
        {
            PType = PacketRegistry.Register(Receive);
        }
       
        internal static void Send(INetEndpoint net, int entityID, bool toggle)
        {
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var id = r.ReadInt32();
            var entity = net.World.GetEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.CrouchToggle(toggle);

            if (net is Server)
                Send(net, entity.RefId, toggle);
        }
    }
}
