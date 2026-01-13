using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerZoneDelete
    {
        static readonly int _pPlayerZoneDelete;
        static PacketPlayerZoneDelete()
        {
            _pPlayerZoneDelete = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(NetEndpoint net, Type zoneType, int zoneID)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write();
            //var w = net.BeginPacketNew(ReliabilityType.OrderedReliable, PacketPlayerZoneDelete);

            net.BeginPacketImmediate(_pPlayerZoneDelete)
                .Write(zoneType.FullName)
                .Write(zoneID);
        }
        public static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            Type zoneType = Type.GetType(r.ReadString());
            int zoneID = r.ReadInt32();
            net.Map.Town.ZoneManager.Delete(zoneID);
            if (net is Server)
                Send(net, zoneType, zoneID);
        }
    }
}
