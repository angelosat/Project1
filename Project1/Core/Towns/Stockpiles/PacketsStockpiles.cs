using Project1.Framework;
using Project1.Core.Base;
using Project1.Core.Helpers.Structs;
using Project1.Core.Materials;
using Project1.Core.Net;
using Project1.Core.Helpers;
using Project1.Core.Entities;
using Project1.Core.Crafting;

namespace Project1.Core.Towns.Stockpiles
{
    [EnsureStaticCtorCall]
    internal static class PacketsStockpiles
    {
        static readonly PacketId _pFiltersChanged;
        static PacketsStockpiles()
        {
            _pFiltersChanged = Registry.PacketHandlers.Register(OnFiltersChanged);

            Registry.PlayerInputEventHooks.Register<PlayerModifiedStockpileFiltersEvent>(HandlePlayerModifiedStockpileFilters);
        }

        private static void HandlePlayerModifiedStockpileFilters(PlayerModifiedStockpileFiltersEvent e)
        {
            var net = Client.Instance;
            var item = e.Item;
            var stockpile = e.Stockpile;
            var profile = e.Profile;
            var mat = e.Material;
            SendStockpileFiltersChanged(net, stockpile, item, profile, mat);
        }

        private static void SendStockpileFiltersChanged(NetEndpoint net, Stockpile stockpile, ItemDef itemdef, Def profile, MaterialDef mat)
        {
            net.BeginPacketImmediate(_pFiltersChanged)
                .Write(stockpile.ID)
                .Write(itemdef?.Name ?? "")
                .Write(profile?.Name ?? "")
                .Write(mat?.Name ?? "");
        }

        private static void OnFiltersChanged(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var stockpile = endpoint.Map.Town.ZoneManager.GetZone<Stockpile>(r.ReadInt32());
            var item = r.ReadDef<ItemDef>();
            var profile = r.TryReadDef<Def>();
            var mat = r.TryReadDef<MaterialDef>();
            stockpile.Toggle(item, profile, mat);
            if (endpoint.IsServer)
                SendStockpileFiltersChanged(endpoint, stockpile, item, profile, mat);
        }
    }
}
