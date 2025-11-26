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

        static void Receive(INetwork net, PlayerData player, BinaryReader r)
        {
            var acksCount = r.ReadInt32();
            for (int i = 0; i < acksCount; i++)
            {
                long ackID = r.ReadInt64();
                if (player.WaitingForAck.TryRemove(ackID, out Packet existing))
                {
                    existing.RTT.Stop();
                    player.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds);
                    player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
                    if (player.OrderedPackets.Count > 0)
                        if (player.OrderedPackets.Peek().ID == ackID)
                            player.OrderedPackets.Dequeue();
                }
            }
        }
        internal static void Send(INetwork net)
        {
            if (net is Client client)
            {
                if (client.AckQueue.IsEmpty)
                    return;
                client.OutgoingStream.Write(p);
                client.OutgoingStream.Write(client.AckQueue.Count);
                while (!client.AckQueue.IsEmpty)
                {
                    if (client.AckQueue.TryDequeue(out long id))
                        client.OutgoingStream.Write(id);
                }
            }
        }
        internal static void Init() { }
    }
}
