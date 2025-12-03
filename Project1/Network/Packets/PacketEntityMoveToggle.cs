using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityMoveToggle
    {
        static readonly int PType;
        static PacketEntityMoveToggle()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        
        internal static void Send(INetEndpoint net, int entityID, bool toggle)
        {
            var server = net as Server;
            var w = server.BeginPacket(ReliabilityType.OrderedReliable, PType);
            w.Write(entityID);
            w.Write(toggle);
            server.EndPacket();
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.MoveToggle(toggle);
            if (net is Server)
                Send(net, entity.RefId, toggle);
        }
    }
}
