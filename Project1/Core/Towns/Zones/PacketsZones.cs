using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;

namespace Project1.Core.Towns.Zones;

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
        var mapid = (MapId)r.ReadInt32();
        var zoneID = (ZoneId)r.ReadInt32();
        var map = endpoint.World.Get(mapid);
        map.Town.ZoneManager.DeleteZone(zoneID);
        if (endpoint is Server)
            SendDelete(endpoint, mapid, zoneID);
    }

    private static void OnPlayerDeletingZone(PlayerDeletingZoneEvent e)
    {
        var map = e.Zone.Map;
        if (Ingame.Net.IsServer)
            map.Town.ZoneManager.DeleteZone(e.Zone);
        else
            SendDelete(Ingame.Net, map.ID, e.Zone.ID);
    }
    static public void SendDelete(NetEndpoint net, MapId mapid, ZoneId zoneID)
    {
        net.BeginPacketImmediate(_pPlayerZoneDelete)
            .Write(mapid)
            .Write(zoneID);
    }
    private static void OnPlayerAddingZone(PlayerAddingZoneEvent e)
    {
        var map = Ingame.Net.World.Get(e.MapId);
        if(Ingame.Net.IsServer)
            map.Town.ZoneManager.PlayerEdit(e.ZoneId, e.Def, e.Begin, e.End, e.IsRemoval);
        else
            SendAdd(Ingame.Net, e.ZoneId, e.Def, map.ID, e.Begin, e.End, e.IsRemoval);
    }

    static public void SendAdd(NetEndpoint net, ZoneId zoneID, ZoneDef zoneDef, MapId mapid, IntVec3 begin, IntVec3 end, bool remove)
    {
        net.BeginPacketImmediate(_pPlayerZoneAdd)
            .Write(zoneDef)
            .Write(zoneID)
            .Write(mapid)
            .Write(begin)
            .Write(end)
            .Write(remove);
    }
    static public void ReceiveAdd(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var zoneDef = r.ReadDef<ZoneDef>();
        var zoneID = (ZoneId)r.ReadInt32();
        var mapid = (MapId)r.ReadInt32();
        var begin = r.ReadIntVec3();
        var end = r.ReadIntVec3();
        var remove = r.ReadBoolean();
        var map = net.World.Get(mapid);
        map.Town.ZoneManager.PlayerEdit(zoneID, zoneDef, begin, end, remove);
        if (net is Server)
            SendAdd(net, zoneID, zoneDef, mapid, begin, end, remove);
    }
}
