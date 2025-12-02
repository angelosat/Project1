using Start_a_Town_.Net;
using System;
using System.IO;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketAcks
    {
        static readonly int p;
        static PacketAcks()
        {
            p = Network.RegisterPacketHandler(Receive);
        }

        static void Receive(INetEndpoint net, PlayerData player, BinaryReader r)
        {
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

            //if (net is Client client)
            //{
            //var stream = client.OutgoingStream;
            // the client will write the whole batch of the acks to send back to the server
            var stream = client.OutgoingStreamUnreliable;
            var ackqueue = player.AckQueue;
            if (ackqueue.IsEmpty)
                return;
            stream.Write(p);
            stream.Write(ackqueue.Count);
            while (!ackqueue.IsEmpty)
            {
                if (ackqueue.TryDequeue(out long id))
                    stream.Write(id);
            }
            //}
            //else if (net is Server server)
            //{
            //    if(player is null)
            //        throw new Exception("Server handler called without player");
            //    //var stream = server.GetOutgoingStream();
            //    // TODO: the server will write all the packetids that it received from the player last frame?
            //}
        }
        internal static void Send(BinaryWriter stream, PlayerData player)
        {
            var ackqueue = player.AckQueue;
            if (ackqueue.IsEmpty)
                return;
            stream.Write(p);
            stream.Write(ackqueue.Count);
            while (!ackqueue.IsEmpty)
            {
                if (ackqueue.TryDequeue(out long id))
                    stream.Write(id);
            }
           
        }
        internal static void Init() { }
    }
}
