using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntitySprintToggle
    {
        static readonly int PType;
        static PacketEntitySprintToggle()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        
        internal static void Send(INetEndpoint net, int entityID, bool toggle)
        {
            var server = net as Server;
            //var w = server.OutgoingStreamTimestamped;
            var w = server[ReliabilityType.OrderedReliable];
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.SprintToggle(toggle);

            if (net is Server)
                Send(net, entity.RefId, toggle);
        }
    }
}
