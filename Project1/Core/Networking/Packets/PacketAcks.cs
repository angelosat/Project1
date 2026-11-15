using Project1.Core.Networking;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Project1.Core.Networking.Packets
{
    [EnsureStaticCtorCall]
    static class PacketAcks
    {
        static readonly int p;
        static PacketAcks()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }

        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var player = pck.Player;
            var acksCount = r.ReadInt32();
            for (int i = 0; i < acksCount; i++)
            {
                long ackID = r.ReadInt64();
                if (player.WaitingForAck.TryRemove(ackID, out Packet existing))
                {
                    existing.RTT.Stop();
                    if(net is Server)
                        player.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds); // because the player instance in the client has connection = null (for now?)
                    player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
                    if (player.OrderedPackets.Count > 0)
                        if (player.OrderedPackets.Peek().ID == ackID)
                            player.OrderedPackets.Dequeue();
                }
            }
        }
        internal static void Send(PlayerData player)
        {
            Send(player.StreamUnreliable, player);
        }

        internal static void Send(Client client, PlayerData player)
        {
            Send(client.OutgoingStreamUnreliable, player);
        }

        private static void Send(BinaryWriter stream, PlayerData player)
        {
            var ackqueue = player.AckQueue;

            if (ackqueue.IsEmpty)
                return;

            // Capture a fixed batch before writing its count.
            var acks = new List<long>();
            int count = ackqueue.Count;

            for (int i = 0; i < count; i++)
            {
                if (ackqueue.TryDequeue(out long id))
                    acks.Add(id);
            }

            if (acks.Count == 0)
                return;

            stream.Write(p);
            stream.Write(acks.Count);

            foreach (long id in acks)
                stream.Write(id);
        }
        //internal static void Send(PlayerData player)
        //{
        //    Send(player.StreamUnreliable, player);
        //}
        //internal static void Send(Client client, PlayerData player)
        //{

        //    //if (net is Client client)
        //    //{
        //    //var stream = client.OutgoingStream;
        //    // the client will write the whole batch of the acks to send back to the server
        //    var stream = client.OutgoingStreamUnreliable;
        //    var ackqueue = player.AckQueue;
        //    if (ackqueue.IsEmpty)
        //        return;
        //    stream.Write(p);
        //    stream.Write(ackqueue.Count);
        //    while (!ackqueue.IsEmpty)
        //    {
        //        if (ackqueue.TryDequeue(out long id))
        //            stream.Write(id);
        //    }
        //    //}
        //    //else if (net is Server server)
        //    //{
        //    //    if(player is null)
        //    //        throw new Exception("Server handler called without player");
        //    //    //var stream = server.GetOutgoingStream();
        //    //    // TODO: the server will write all the packetids that it received from the player last frame?
        //    //}
        //}
        //internal static void Send(BinaryWriter stream, PlayerData player)
        //{
        //    var ackqueue = player.AckQueue;
        //    if (ackqueue.IsEmpty)
        //        return;
        //    stream.Write(p);
        //    stream.Write(ackqueue.Count);
        //    while (!ackqueue.IsEmpty)
        //    {
        //        if (ackqueue.TryDequeue(out long id))
        //            stream.Write(id);
        //    }

        //}
        //internal static void Init() { }
    }
}
