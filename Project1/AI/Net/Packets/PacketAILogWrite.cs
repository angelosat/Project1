using Start_a_Town_.Net;

namespace Start_a_Town_.Modules.AI.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketAILogWrite
    {
        static readonly int p;
        static PacketAILogWrite()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(Server server, int agentID, string entry)
        {
            server.OutgoingStreamOrderedReliable.Write(p);
            server.OutgoingStreamOrderedReliable.Write(agentID);
            server.OutgoingStreamOrderedReliable.Write(entry);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var entity = net.World.GetEntity(r.ReadInt32()) as Actor;
            var entry = r.ReadString();
            entity.Log.Write(entry);
        }
       
    }
}
