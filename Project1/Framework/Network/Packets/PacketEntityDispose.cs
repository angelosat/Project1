using Start_a_Town_;

namespace Project1.Framework.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketEntityDispose
    {
        static readonly int pPlayerRequest;
        static PacketEntityDispose()
        {
            pPlayerRequest = Registry.PacketHandlers.Register(ReceivePlayerRequest);
        }

        internal static void Send(NetEndpoint net, int entityID, PlayerData player)
        {
            var w = net.BeginPacketImmediate(pPlayerRequest);
            w.Write(player.ID);
            w.Write(entityID);
        }

        private static void ReceivePlayerRequest(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var player = net.GetPlayer(r.ReadInt32());
            var id = r.ReadInt32();
            net.World.DisposeEntity(id);
            if (net is Server server)
                Send(server, id, player);
        }
    }
}
