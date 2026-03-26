using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;

namespace Project1.Core.Towns.Inns
{
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
                Ingame.Net.Map.Town.InnManager.ToggleBed(e.Bed);
            SendToggleInnBed(Ingame.Net, e.Bed);
        }
        private static void SendToggleInnBed(NetEndpoint endpoint, IntVec3 bed)
        {
            endpoint.BeginPacketImmediate(_pToggleInnBed)
                .Write(bed);
        }
        private static void ReceiveToggleInnBed(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var bed = r.ReadIntVec3();
            endpoint.Map.Town.InnManager.ToggleBed(bed);
            if (endpoint.IsServer)
                SendToggleInnBed(endpoint, bed);
        }
    }
}
