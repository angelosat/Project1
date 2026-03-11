using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Legacy.Storage;
using Project1.Core.Networking;
using Project1.Core.Systems.Materials;
using Project1.Framework;

namespace Project1.Core.Towns.Stockpiles
{
    [EnsureStaticCtorCall]
    internal static class PacketsStockpiles
    {
        static readonly PacketId _pFiltersChanged, _packetStockpileSync;

        static PacketsStockpiles()
        {
            _pFiltersChanged = Registry.PacketHandlers.Register(OnFiltersChanged);
            _packetStockpileSync = Registry.PacketHandlers.Register(ReceivePriority);

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

        internal static void SyncPriority(IStorageNew storage, StoragePriority p)
        {
            var stockpile = storage as Stockpile;
            var net = stockpile.Map.Net;
            if (net is Server)
                stockpile.Settings.Priority = p;
            var w = stockpile.Map.Net.BeginPacket(_packetStockpileSync);

            w.Write(stockpile.ID);
            w.Write((byte)p);
        }
        private static void ReceivePriority(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var stockpileID = r.ReadInt32();
            var p = r.ReadByte();
            var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);
            var newPriority = (StoragePriority)p;
            if (net is Server)
                SyncPriority(stockpile, newPriority);
            else
                stockpile.Settings.Priority = newPriority;
        }
    }
}
