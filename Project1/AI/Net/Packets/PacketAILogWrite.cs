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
            server.OutgoingStream.Write(p);
            server.OutgoingStream.Write(agentID);
            server.OutgoingStream.Write(entry);
        }
        static public void Receive(INetPeer net, BinaryReader r)
        {
            var entity = net.GetNetworkEntity(r.ReadInt32()) as Actor;
            var entry = r.ReadString();
            entity.Log.Write(entry);
        }
       
    }
}
