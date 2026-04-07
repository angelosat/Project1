using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Systems.Plants
{
    [EnsureStaticCtorCall]
    static class PacketsGrowingZones
    {
        static readonly int pSync;
        static PacketsGrowingZones()
        {
            pSync = Registry.PacketHandlers.Register(Sync);
        }
        public static void Send(GrowingZone zone, PlantSpeciesDef plant, bool tilling, bool planting, bool harvesting)
        {
            var client = zone.Net as Client;
            var w = client.GetOutgoingStreamOrderedReliable();
            w.Write(pSync);
            w.Write(zone.Map.ID);
            w.Write(zone.ID);
            plant.Write(w);
            w.Write(tilling);
            w.Write(planting);
            w.Write(harvesting);
        }
        public static void SendPlant(GrowingZone zone, PlantSpeciesDef plant)
        {
            Send(zone, plant, zone.Tilling, zone.Planting, zone.Harvesting);
        }
        public static void ToggleTilling(GrowingZone zone)
        {
            Send(zone, zone.Plant, !zone.Tilling, zone.Planting, zone.Harvesting);
        }
        public static void TogglePlanting(GrowingZone zone)
        {
            Send(zone, zone.Plant, zone.Tilling, !zone.Planting, zone.Harvesting);
        }
        public static void ToggleHarvesting(GrowingZone zone)
        {
            Send(zone, zone.Plant, zone.Tilling, zone.Planting, !zone.Harvesting);

        }
        static void Sync(GrowingZone zone)
        {
            //if (zone.Net is Client)
            //    return;

            //var w = zone.Map.Net.GetOutgoingStreamOrderedReliable();
            //w.Write(pSync);
            var w = zone.Map.Net.BeginPacketOld(pSync);
            w.Write(zone.Map.ID);
            w.Write(zone.ID);
            zone.Plant.Write(w);
            w.Write(zone.Tilling);
            w.Write(zone.Planting);
            w.Write(zone.Harvesting);
        }
        static void Sync(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
            var mapid = r.ReadMapId();
            var map = net.World.Get(mapid);
            var zone = map.Town.ZoneManager.GetZone<GrowingZone>(r.ReadInt32());
            zone.Plant = Def.Get<PlantSpeciesDef>(r);
            zone.Tilling = r.ReadBoolean();
            zone.Planting = r.ReadBoolean();
            zone.Harvesting = r.ReadBoolean();
            if (net is Server server)
                Sync(zone);
        }
    }
}
