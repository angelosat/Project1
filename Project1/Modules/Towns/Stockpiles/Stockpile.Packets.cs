using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public partial class Stockpile
    {
        [EnsureStaticCtorCall]
        class Packets
        {
            static readonly int PacketStockpileSync;
            static Packets()
            {
                PacketStockpileSync = Network.RegisterPacketHandler(Receive);
            }
            internal static void SyncPriority(IStorageNew storage, StoragePriority p)
            {
                var stockpile = storage as Stockpile;
                var net = stockpile.Map.Net;
                if (net is Server)
                    stockpile.Settings.Priority = p;
                var w = net.GetOutgoingStreamOrderedReliable();
                w.Write(PacketStockpileSync);
                w.Write(stockpile.ID);
                w.Write((byte)p);
            }
            private static void Receive(INetEndpoint net, BinaryReader r)
            {
                var stockpileID = r.ReadInt32();
                var p = r.ReadByte();
                var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);
                var newPriority = (StoragePriority)p;
                if (net is Server)
                    SyncPriority(stockpile, newPriority);
                else
                    stockpile.Settings.Priority = newPriority;
            }
        }
    }
}
