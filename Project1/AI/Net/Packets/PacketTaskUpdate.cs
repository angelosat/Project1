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
            p = Registry.PacketHandlers.Register(PacketTaskUpdate.Receive);
        }
        static public void Send(Server server, int agentID, string taskString)
        {
            server.OutgoingStreamOrderedReliable.Write(p);
            server.OutgoingStreamOrderedReliable.Write(agentID);
            server.OutgoingStreamOrderedReliable.Write(taskString);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var entity = net.World.GetEntity(r.ReadInt32());
            if (entity == null)
                return;
            var taskString = r.ReadString();
            AIState.GetState(entity).TaskString = taskString;
        }
    }
}
