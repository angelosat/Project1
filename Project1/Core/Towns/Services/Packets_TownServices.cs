using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Towns.Services;

[EnsureStaticCtorCall]
internal class Packets_TownServices
{
    static readonly PacketId
        _pSync = Registry.PacketHandlers.Register(ReceiveSync),
        _pCounterServiceAssigned = Registry.PacketHandlers.Register(ReceiveCounterServiceAssigned);

   

    static Packets_TownServices()
    {
        Registry.MapEventHooksServer.Register<TownServiceRequestUpdatedEvent>(HandleRequestUpdated);

        Registry.PlayerInputEventHooks.Register<PlayerAssignedServiceToCounterEvent>(HandlePlayerAssignedServiceToCounter);
    }

    private static void HandlePlayerAssignedServiceToCounter(PlayerAssignedServiceToCounterEvent e)
    {
        if(Ingame.Net.IsServer)
            e.Comp.SetService(e.Service);
        SendPlayerCounterServiceAssigned(Ingame.Net, e.Comp.Parent.Map.ID, e.Comp.Parent.OriginGlobal, e.Service);
    }

    private static void SendPlayerCounterServiceAssigned(NetEndpoint endpoint, MapId mapid, IntVec3 global, TownServiceDef service)
    {
        endpoint.BeginPacketImmediate(_pCounterServiceAssigned)
            .Write(mapid)
            .Write(global)
            .Write(service);
    }
    private static void ReceiveCounterServiceAssigned(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var map = endpoint.World.Get(r.ReadMapId());
        var comp = map.GetBlockEntityComp<BlockShopComp>(r.ReadIntVec3());
        var service = r.ReadDef<TownServiceDef>();
        comp.SetService(service);
        if (endpoint is Server server)
            SendPlayerCounterServiceAssigned(server, map.ID, comp.Parent.OriginGlobal, service);

    }

    private static void HandleRequestUpdated(TownServiceRequestUpdatedEvent e)
    {
        SendRequestUpdated(Server.Instance, e.Map, e.Request);
    }

    private static void SendRequestUpdated(NetEndpoint endpoint, MapBase map, ServiceRequest request)
    {
        var w = endpoint.BeginPacket(_pSync)
            .Write(map.ID)
            .Write(request.Id);
        request.Write(w);
    }

    private static void ReceiveSync(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var map = endpoint.World.Get(r.ReadMapId());
        var manager = map.Town.ServiceRequests;
        var request = manager.Get(r.ReadUInt64());
        request.Read(r);
    }
}
