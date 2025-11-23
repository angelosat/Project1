using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityJump
    {
        static readonly int PType;
        static PacketEntityJump()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        
        internal static void Send(INetwork net, int entityID)
        {
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(PType);
            w.Write(entityID);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            entity.Jump();
            if (net is Server)
                Send(net, entity.RefID);
        }
    }
}
