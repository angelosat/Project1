using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityDispose
    {
        static readonly int pServerAction, pPlayerRequest;
        static PacketEntityDispose()
        {
            pServerAction = Registry.PacketHandlers.Register(Receive);
            pPlayerRequest = Registry.PacketHandlers.Register(ReceivePlayer);
        }

        internal static void Send(Server server, int entityID, PlayerData player)
        {
            var w = player is null ? server.OutgoingStreamTimestamped : server.GetOutgoingStreamOrderedReliable();
            w.Write(pServerAction);
            w.Write(entityID);
        }
        internal static void Send(Client client, int entityID, PlayerData player)
        {
            var w = client.GetOutgoingStreamOrderedReliable();
            w.Write(pPlayerRequest);
            w.Write(entityID);
            w.Write(player.ID);
        }
       
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var id = r.ReadInt32();
            net.DisposeObject(id);
            if (net is Server)
                throw new System.Exception(); // this should only be handled by clients
        }

        private static void ReceivePlayer(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var id = r.ReadInt32();
            var player = net.GetPlayer(r.ReadInt32());
            net.DisposeObject(id);
            if (net is Server server)
                Send(server, id, player);
            else
                throw new System.Exception(); // this shouldn't be handled by clients
        }
    }
}
