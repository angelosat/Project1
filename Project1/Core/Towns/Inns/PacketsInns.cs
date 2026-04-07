using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;

namespace Project1.Core.Towns.Inns;

[EnsureStaticCtorCall]
internal static class PacketsInns
{
    internal static PacketId _pToggleInnBed = Registry.PacketHandlers.Register(ReceiveToggleInnBed);
    static PacketsInns()
    {
        Registry.PlayerInputEventHooks.Register<PlayerToggledInnBedEvent>(HandlePlayerToggledInnBed);
    }

    private static void HandlePlayerToggledInnBed(PlayerToggledInnBedEvent e)
    {
        if(Ingame.Net.IsServer)
            Ingame.Net.World.Get(e.MapId).Town.InnManager.ToggleBed(e.Bed);
        SendToggleInnBed(Ingame.Net, e.MapId, e.Bed);
    }
    private static void SendToggleInnBed(NetEndpoint endpoint, MapId mapId, IntVec3 bed)
    {
        endpoint.BeginPacketImmediate(_pToggleInnBed)
            .Write(mapId)
            .Write(bed);
    }
    private static void ReceiveToggleInnBed(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var mapid = r.ReadMapId();
        var map = endpoint.World.Get(mapid);
        var bed = r.ReadIntVec3();
        map.Town.InnManager.ToggleBed(bed);
        if (endpoint.IsServer)
            SendToggleInnBed(endpoint, mapid, bed);
    }
}
