using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketChunk
    {
        static int p;
        static PacketChunk()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(INetEndpoint net, Vector2 vector2, byte[] chunkData, PlayerData player)
        {
            var server = net as Server;
            var w = player.StreamReliable;
            w.Write(p);
            w.Write(chunkData);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var chunk = Chunk.Create(net.Map, r);
            var client = net as Client;
            client.ReceiveChunk(chunk);
            ("chunk received " + chunk.MapCoords.ToString()).ToConsole();
            PacketChunkReceived.Send(client, Client.Instance.PlayerData, chunk.MapCoords);
            // change screen when player entity is assigned instead of here?
            if(net.Map.AreChunksLoaded)
                GameMode.Current.AllChunksReceived(net);
        }
    }
}
