using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketTaskUpdate
    {
        static readonly int p;
        static PacketTaskUpdate()
        {
            p = Network.RegisterPacketHandler(PacketTaskUpdate.Receive);
        }
        static public void Send(Server server, int agentID, string taskString)
        {
            server.OutgoingStream.Write(p);
            server.OutgoingStream.Write(agentID);
            server.OutgoingStream.Write(taskString);
        }
        static public void Receive(INetEndpoint net, BinaryReader r)
        {
            var entity = net.GetNetworkEntity(r.ReadInt32());
            if (entity == null)
                return;
            var taskString = r.ReadString();
            AIState.GetState(entity).TaskString = taskString;
        }
    }
}
