using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Modules.AI.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketAILogWrite
    {
        static readonly int p;
        static PacketAILogWrite()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(Server server, int agentID, string entry)
        {
            server.OutgoingStreamOrderedReliable.Write(p);
            server.OutgoingStreamOrderedReliable.Write(agentID);
            server.OutgoingStreamOrderedReliable.Write(entry);
        }
        static public void Receive(INetEndpoint net, BinaryReader r)
        {
            var entity = net.GetNetworkEntity(r.ReadInt32()) as Actor;
            var entry = r.ReadString();
            entity.Log.Write(entry);
        }
       
    }
}
