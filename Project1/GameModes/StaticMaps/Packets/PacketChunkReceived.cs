using System.IO;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class PacketChunkReceived
    {
        static readonly int _packetTypeId;
        static PacketChunkReceived()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Init()
        {
        }
        internal static void Send(Client client, PlayerData player, Vector2 chunkCoords)
        {
            var w = client.BeginPacket(_packetTypeId);
            w.Write(player.ID);
            w.Write(chunkCoords);
        }
        internal static void Receive(NetEndpoint net, Packet p)
        {
            var r = p.PacketReader;
            var playerid = r.ReadInt32();
            var vec = r.ReadVector2();
            GameMode.Current.ChunkReceived(net as Server, playerid, vec);
        }
    }
}
