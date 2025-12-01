using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class GrowingZone : Zone, IContextable, ISelectable
    {
        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pSync;
            static Packets()
            {
                pSync = Network.RegisterPacketHandler(Sync);
            }
            public static void Send(GrowingZone zone, PlantProperties plant, bool tilling, bool planting, bool harvesting)
            {
                var client = zone.Net as Client;
                var w = client.GetOutgoingStream();
                w.Write(pSync);
                w.Write(zone.ID);
                plant.Write(w);
                w.Write(tilling);
                w.Write(planting);
                w.Write(harvesting);
            }
            public static void SendPlant(GrowingZone zone, PlantProperties plant)
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

                var w = zone.Map.Net.GetOutgoingStream();
                w.Write(pSync);
                w.Write(zone.ID);
                zone.Plant.Write(w);
                w.Write(zone.Tilling);
                w.Write(zone.Planting);
                w.Write(zone.Harvesting);
            }
            static void Sync(INetEndpoint net, BinaryReader r)
            {
                var zone = net.Map.Town.ZoneManager.GetZone<GrowingZone>(r.ReadInt32());
                zone.Plant = Def.GetDef<PlantProperties>(r);
                zone.Tilling = r.ReadBoolean();
                zone.Planting = r.ReadBoolean();
                zone.Harvesting = r.ReadBoolean();
                if (net is Server server)
                    Sync(zone);
            }
        }

        internal bool IsValidTilling(IntVec3 global)
        {
            return this.CachedTilling.Contains(global);
        }

        public bool Harvesting = true;
        public bool Planting = true;
        public bool Tilling = true;
        public PlantProperties Plant = PlantProperties.Berry;
        public float HarvestThreshold = 1;
        public override string UniqueName => $"Zone_Growing_{this.ID}";
        public ItemDef SeedType = PlantDefOf.Bush;
        readonly HashSet<IntVec3> CachedTilling = new();
        readonly HashSet<IntVec3> CachedSowing = new();
        bool Valid;

        public GrowingZone(BinaryReader r)
            : base()
        {
            this.Read(r);
        }

        public GrowingZone(ZoneManager manager) : base(manager)
        {
        }
        public GrowingZone(ZoneManager manager, IEnumerable<IntVec3> positions)
            : base(manager, positions)
        {
        }
        public override ZoneDef ZoneDef => ZoneDefOf.Growing;

        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Tilling);
            w.Write(this.Planting);
            w.Write(this.Harvesting);
            this.Plant.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Tilling = r.ReadBoolean();
            this.Planting = r.ReadBoolean();
            this.Harvesting = r.ReadBoolean();
            this.Plant = r.ReadDef<PlantProperties>();
        }

        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValueNew("Tilling", ref this.Tilling);
            tag.TryGetTagValueNew("Planting", ref this.Planting);
            tag.TryGetTagValueNew("Harvesting", ref this.Harvesting);
            tag.TryLoadDef("Plant", ref this.Plant);
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Tilling.Save("Tilling"));
            tag.Add(this.Planting.Save("Planting"));
            tag.Add(this.Harvesting.Save("Harvesting"));
            tag.Add(this.Plant.Save("Plant"));
        }

        internal override void OnBlockChanged(IntVec3 global)
        {
            var below = global.Below;
            var map = this.Map;
            if (this.Positions.Contains(global))
            {
                //if (!Block.IsBlockSolid(map, global) || map.GetMaterial(global) != MaterialDefOf.Soil)
                if(map.GetCell(global).Material != MaterialDefOf.Soil)
                {
                    this.RemovePosition(global);
                    return;
                }
            }
            else if (this.Positions.Contains(below))
            {
                if (!map.IsAir(global))
                {
                    this.RemovePosition(below);
                    return;
                }
            }
        }

        public IEnumerable<IntVec3> GetSowingPositions()
        {
            if (!this.Valid)
                this.Validate();
            foreach (var pos in this.CachedSowing)
                yield return pos;
        }
        public IEnumerable<IntVec3> GetSowingPositions(int spacing)
        {
            if (!this.Valid)
                this.Validate();
            var first = this.Positions.First();
            foreach(var pos in this.CachedSowing)
            { 
                var d = pos - first;
                if (d.X % (spacing + 1) == 0 && d.Y % (spacing + 1) == 0)
                    yield return pos;
            }
        }
        public IEnumerable<IntVec3> GetTillingPositions()
        {
            if (!this.Valid)
                this.Validate();
            foreach (var pos in this.CachedTilling)
                yield return pos;
        }

        protected override void Validate()
        {
            this.Valid = true;
            this.CachedTilling.Clear();
            this.CachedSowing.Clear();
            foreach (var pos in this.Positions)
            {
                var cell = this.Town.Map.GetCell(pos);
                var block = cell.Block;
                var cellData = cell.BlockData;
                if (block == BlockDefOf.Farmland)
                {
                    if (!this.Town.Map.GetObjects(pos.Above).Any(o => o.IsPlant()))
                        if (!BlockFarmland.IsSeeded(cellData))
                            this.CachedSowing.Add(pos);
                }
                //else if (block.GetMaterial(cellData) == MaterialDefOf.Soil)
                else if (cell.Material == MaterialDefOf.Soil)
                {
                    if (!this.Town.Map.GetObjects(pos.Above).Any(o => o.IsPlant()))
                        this.CachedTilling.Add(pos);
                }
            }
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a) { }

        public static bool IsValidFarmPosition(MapBase map, Vector3 arg)
        {
            return
                Block.GetBlockMaterial(map, arg) == MaterialDefOf.Soil
                && map.GetBlock(arg + Vector3.UnitZ) == BlockDefOf.Air;
        }

        internal IEnumerable<Plant> GetHarvestablePlantsLazy()
        {
            foreach (var pos in this.Positions)
            {
                var above = pos.Above;
                var grownPlants = this.Town.Map.GetObjects(above).OfType<Plant>().Where(p => p.PlantComponent.IsHarvestable);
                foreach (var obj in grownPlants)
                    yield return obj;
            }
        }
        internal IEnumerable<GameObject> GetHarvestablePlants()
        {
            return this.Town.Map.GetObjects(this.Positions.Select(pos => (Vector3)pos.Above)).OfType<Plant>().Where(p => p.IsHarvestable);
        }
        internal IEnumerable<GameObject> GetChoppableTrees()
        {
            return this.Town.Map.GetObjects(this.Positions.Select(pos => (Vector3)pos.Above)).Where(TreeComponent.IsGrown);
        }
        public bool IsValidSeed(GameObject item)
        {
            return this.Plant is not null && item.IsSeedFor(this.Plant);
        }
        public override string GetName()
        {
            return this.Name;
        }
        public override IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield return ("Plant", this.ToggleGui);
        }
        static Control gui;
        void ToggleGui()
        {
            (gui ??= createGui())
                .GetData(this)
                .Show();

            static Control createGui()
            {
                GrowingZone growzone = null;
                var box = new GroupBox();// 300, 200);
                box.AddControlsVertically(
                    new ComboBoxNewNew<PlantProperties>(Def.GetDefs<PlantProperties>(), 128, $"Plant: ", d => $"{d?.Label ?? ""}", () => growzone?.Plant, p => Packets.SendPlant(growzone, p)),
                    new CheckBoxNew("Tilling", () => Packets.ToggleTilling(growzone), () => growzone.Tilling),
                    new CheckBoxNew("Planting", () => Packets.TogglePlanting(growzone), () => growzone.Planting),
                    new CheckBoxNew("Harvesting", () => Packets.ToggleHarvesting(growzone), () => growzone.Harvesting)

                    //new ComboBoxNewNew<PlantProperties>(Def.GetDefs<PlantProperties>(), 128, $"Plant: ", d => $"{d?.Label ?? ""}", () => growzone?.Plant, selectPlant),
                    //new CheckBoxNew("Tilling", () => growzone.Tilling = !growzone.Tilling, () => growzone.Tilling),
                    //new CheckBoxNew("Planting", () => growzone.Planting = !growzone.Planting, () => growzone.Planting),
                    //new CheckBoxNew("Harvesting", () => growzone.Harvesting = !growzone.Harvesting, () => growzone.Harvesting)
                    );
                var win = box.ToWindow();
                win.SetGetDataAction(o =>
                {
                    growzone = o as GrowingZone;
                    win.SetTitle(growzone.Name);
                });
                win.SetOnSelectedTargetChangedAction(t =>
                {
                    if (t.Type != TargetType.Position)
                        return;
                    if (t.Map.Town.ZoneManager.GetZoneAt<GrowingZone>(t.Global) is not GrowingZone gz)
                        return;
                    win.GetData(gz);
                });
                return box.Window;
            }
        }

        public override bool Accepts(Entity obj, IntVec3 pos)
        {
            if (this.Plant is null)
                return false;
            if (obj.Def != ItemDefOf.Seeds)
                return false;
            if (!this.CachedSowing.Contains(pos))
                return false;
            return obj.IsSeedFor(this.Plant);
        }
    }
}
