using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using System;

namespace Project1.Core.Networking.Simulation;

class PacketsMap
{
    static readonly int
        PacketSyncSetCellData;
    static PacketsMap()
    {
        PacketSyncSetCellData = Registry.PacketHandlers.Register(SyncSetCellData);
    }
    
    public static void SyncSetCellData(MapBase map, IntVec3 global, byte data)
    {
        var net = map.Net as Server;
        if (net is not Server server)
            throw new Exception();
        map.SetCellData(global, data);
        net.BeginPacket(PacketSyncSetCellData)
            .Write(map.ID)
            .Write(global)
            .Write(data);
    }
    private static void SyncSetCellData(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var map = net.World.Get(r.ReadMapId());
        var global = r.ReadIntVec3();
        var data = r.ReadByte();
        if (net is Client)
            map.SetCellData(global, data);
        else
            SyncSetCellData(map, global, data);
    }
}
