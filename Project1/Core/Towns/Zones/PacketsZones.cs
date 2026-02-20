using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.Towns.Zones
{
    [EnsureStaticCtorCall]
    static class PacketsZones
    {
        static readonly PacketId _pPlayerZoneAdd, _pPlayerZoneDelete;
        static PacketsZones()
        {
            _pPlayerZoneAdd = Registry.PacketHandlers.Register(ReceiveAdd);
            _pPlayerZoneDelete = Registry.PacketHandlers.Register(ReceiveDelete);
            Registry.PlayerInputEventHooks.Register<PlayerAddingZoneEvent>(OnPlayerAddingZone);
            Registry.PlayerInputEventHooks.Register<PlayerDeletingZoneEvent>(OnPlayerDeletingZone);
        }

        private static void ReceiveDelete(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var zoneID = (ZoneId)r.ReadInt32();
            endpoint.Map.Town.ZoneManager.DeleteZone(zoneID);
            if (endpoint is Server)
                SendDelete(endpoint, zoneID);
        }

        private static void OnPlayerDeletingZone(PlayerDeletingZoneEvent e)
        {
            if (Ingame.Net.IsServer)
                Ingame.Net.Map.Town.ZoneManager.DeleteZone(e.Zone);
            else
                SendDelete(Ingame.Net, e.Zone.ID);
        }
        static public void SendDelete(NetEndpoint net, ZoneId zoneID)
        {
            net.BeginPacketImmediate(_pPlayerZoneDelete)
                .Write(zoneID);
        }
        private static void OnPlayerAddingZone(PlayerAddingZoneEvent e)
        {
            if(Ingame.Net.IsServer)
                Ingame.Net.Map.Town.ZoneManager.PlayerEdit(e.ZoneId, e.Def, e.Begin, e.End, e.IsRemoval);
            else
                SendAdd(Ingame.Net, e.ZoneId, e.Def, e.Begin, e.End, e.IsRemoval);
        }

        static public void SendAdd(NetEndpoint net, ZoneId zoneID, ZoneDef zoneDef, IntVec3 begin, IntVec3 end, bool remove)
        {
            net.BeginPacketImmediate(_pPlayerZoneAdd)
                .Write(zoneDef)
                .Write(zoneID)
                .Write(begin)
                .Write(end)
                .Write(remove);
        }
        static public void ReceiveAdd(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var zoneDef = r.ReadDef<ZoneDef>();
            var zoneID = (ZoneId)r.ReadInt32();
            var begin = r.ReadIntVec3();
            var end = r.ReadIntVec3();
            var remove = r.ReadBoolean();
            net.Map.Town.ZoneManager.PlayerEdit(zoneID, zoneDef, begin, end, remove);
            if (net is Server)
                SendAdd(net, zoneID, zoneDef, begin, end, remove);
        }
    }
}
