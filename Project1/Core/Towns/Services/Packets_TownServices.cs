using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Towns.Services;

[EnsureStaticCtorCall]
internal class Packets_TownServices
{
    static readonly PacketId
        _pSync = Registry.PacketHandlers.Register(ReceiveSync);

    static Packets_TownServices()
    {
        Registry.MapEventHooksServer.Register<TownServiceRequestUpdatedEvent>(HandleRequestUpdated);
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
