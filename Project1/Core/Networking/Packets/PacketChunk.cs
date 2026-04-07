using Microsoft.Xna.Framework;
using Project1.Core.Helpers;
using Project1.Core.Networking.Simulation;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Networking.Packets;

[EnsureStaticCtorCall]
static class PacketChunk
{
    static int p;
    static PacketChunk()
    {
        p = Registry.PacketHandlers.Register(Receive);
    }
    internal static void Send(INetEndpoint net, Vector2 vector2, /*MapBase map, */byte[] chunkData, PlayerData player)
    {
        var server = net as Server;
        var w = player.BeginReliable(p);
        //w.Write(map.ID);
        w.Write(chunkData);
    }
    internal static void Receive(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        //var map = net.World.Get(r.ReadMapId());
        var chunk = Chunk.Create(net.Map, r);
        //var chunk = Chunk.Create(map, r);
        var client = net as Client;
        client.ReceiveChunk(chunk);
        ("chunk received " + chunk.MapCoords.ToString()).ToConsole();
        PacketChunkReceived.Send(client, Client.Instance.PlayerData, chunk.MapCoords);
        // change screen when player entity is assigned instead of here?
        if(net.Map.AreChunksLoaded)
            GameMode.Current.AllChunksReceived(net);
    }
}
