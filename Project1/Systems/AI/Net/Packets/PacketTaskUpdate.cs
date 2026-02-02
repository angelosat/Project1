using Start_a_Town_.AI;
using Project1.Framework.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketTaskUpdate
    {
        static readonly int _packetTypeId;
        static PacketTaskUpdate()
        {
            _packetTypeId = Registry.PacketHandlers.Register(PacketTaskUpdate.Receive);
        }
        static public void Send(Server server, int agentID, string taskString)
        {
            server.BeginPacket(_packetTypeId)
                .Write(agentID)
                .Write(taskString);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            if (net is not Client client)
                throw new System.Exception();
            var r = pck.PacketReader;
            var entity = net.World.GetEntity(r.ReadInt32());
            if (entity == null)
                return;
            var taskString = r.ReadString();
            AIState.GetState(entity).TaskString = taskString;
        }
    }
}
