using System;
using Project1.Framework;
using Project1.Core.Networking;
using Project1.Framework.Events;
using Project1.Core.Networking;

namespace Project1.Core.Towns.Zones
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
            net.BeginPacketImmediate(_pPlayerZoneDelete)
                .Write(zoneType.FullName)
                .Write(zoneID);
        }
        public static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            Type zoneType = Type.GetType(r.ReadString());
            int zoneID = r.ReadInt32();
            net.Map.Town.ZoneManager.DeleteZone(zoneID);
            if (net is Server)
                Send(net, zoneType, zoneID);
        }
    }
}
