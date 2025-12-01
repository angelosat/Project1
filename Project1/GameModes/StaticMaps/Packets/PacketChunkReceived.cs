using System.IO;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class PacketChunkReceived
    {
        static readonly int p;
        static PacketChunkReceived()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Init()
        {
        }
        internal static void Send(Client client, PlayerData player, Vector2 chunkCoords)
        {
            var w = client.OutgoingStreamUnreliable;
            w.Write(p);
            w.Write(player.ID);
            w.Write(chunkCoords);
        }
        internal static void Receive(INetEndpoint net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var vec = r.ReadVector2();
            GameMode.Current.ChunkReceived(net as Server, playerid, vec);
        }
    }
}
