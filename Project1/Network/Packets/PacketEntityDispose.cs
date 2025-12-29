using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityDispose
    {
        static readonly int pPlayerRequest; //pServerAction, 
        static PacketEntityDispose()
        {
            pPlayerRequest = Registry.PacketHandlers.Register(ReceivePlayerRequest);
        }

        internal static void Send(Client client, int entityID, PlayerData player)
        {
            var w = client.BeginPacket(pPlayerRequest);
            w.Write(player.ID);
            w.Write(entityID);
        }

        private static void ReceivePlayerRequest(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var player = net.GetPlayer(r.ReadInt32());
            var id = r.ReadInt32();
            net.DisposeObject(id);
        }
    }
}
