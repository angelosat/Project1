using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketServerHandshake
    {
        //static int ServerHandshake;
        //internal static void Init()
        //{
        //    throw new NotImplementedException();
        //    ServerHandshake = Network.RegisterPacketHandler(Receive);
        //}
        //internal static void Send(Server server, PlayerData player)
        //{
        //    throw new NotImplementedException();
        //    var w = server.OutgoingStream;
        //    w.Write(ServerHandshake);
        //    w.Write(player.ID);
        //    server.Players.Write(w);
        //    w.Write(server.Speed);
        //}
        //internal static void Receive(INetwork net, BinaryReader r)
        //{
        //    throw new NotImplementedException();
        //    var client = net as Client;
        //    if (client == null)
        //        throw new Exception();
        //    var playerID = r.ReadInt32();
        //    var playerList = PlayerList.Read(net, r);
        //    var speed = r.ReadInt32();
        //    client.HandleServerResponse(playerID, playerList, speed);
        //}
    }
}
