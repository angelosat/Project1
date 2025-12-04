using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketZoneDelete
    {
        static readonly int PacketPlayerZoneDelete;
        static PacketZoneDelete()
        {
            PacketPlayerZoneDelete = Network.RegisterPacketHandler(Receive);
        }
        public static void Send(INetEndpoint net, Type zoneType, int zoneID)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write();
            var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, PacketPlayerZoneDelete);

            w.Write(zoneType.FullName);
            w.Write(zoneID);
        }
        public static void Receive(INetEndpoint net, BinaryReader r)
        {
            Type zoneType = Type.GetType(r.ReadString());
            int zoneID = r.ReadInt32();
            net.Map.Town.ZoneManager.Delete(zoneID);
            if (net is Server)
                Send(net, zoneType, zoneID);
        }
    }
}
