using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework.Blocks;
using Project1.Framework.Net;

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
                pSync = Registry.PacketHandlers.Register(Sync);
            }
            public static void Send(GrowingZone zone, PlantSpeciesDef plant, bool tilling, bool planting, bool harvesting)
            {
                var client = zone.Net as Client;
                var w = client.GetOutgoingStreamOrderedReliable();
                w.Write(pSync);
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

                w.Write(zone.ID);
                zone.Plant.Write(w);
                w.Write(zone.Tilling);
                w.Write(zone.Planting);
                w.Write(zone.Harvesting);
            }
            static void Sync(NetEndpoint net, Packet packet)
            {
                var r = packet.PacketReader;
                var zone = net.Map.Town.ZoneManager.GetZone<GrowingZone>(r.ReadInt32());
                zone.Plant = Def.GetDef<PlantSpeciesDef>(r);
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
        public PlantSpeciesDef Plant = PlantSpeciesDefOf.Berry;
        public float HarvestThreshold = 1;
        public override string UniqueName => $"Zone_Growing_{this.ID}";
        public ItemDef SeedType = PlantDefOf.Bush;
        readonly HashSet<IntVec3> CachedTilling = [];
        readonly HashSet<IntVec3> CachedSowing = [];
        public HashSet<Entity> PlantsHarvestable = [];
        public GrowingZone(IDataReader r)
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

        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.Tilling);
            w.Write(this.Planting);
            w.Write(this.Harvesting);
            this.Plant.Write(w);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Tilling = r.ReadBoolean();
            this.Planting = r.ReadBoolean();
            this.Harvesting = r.ReadBoolean();
            this.Plant = r.ReadDef<PlantSpeciesDef>();
        }

        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue("Tilling", ref this.Tilling);
            tag.TryGetTagValue("Planting", ref this.Planting);
            tag.TryGetTagValue("Harvesting", ref this.Harvesting);
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
            if (this.Cells.Contains(global))
            {
                if(map.GetCell(global).Material != MaterialDefOf.Soil)
                {
                    this.RemovePosition(global);
                    return;
                }
            }
            else if (this.Cells.Contains(below))
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
            this.Validate();
            foreach (var pos in this.CachedSowing)
                yield return pos;
        }
        public IntVec3? GetNextSowingPosition()
        {
            this.Validate();
            return this.CachedSowing.Count > 0 ? this.CachedSowing.First() : null;
        }
        public IEnumerable<IntVec3> GetSowingPositions(int spacing)
        {
            this.Validate();
            var first = this.Cells.First();
            foreach(var pos in this.CachedSowing)
            { 
                var d = pos - first;
                if (d.X % (spacing + 1) == 0 && d.Y % (spacing + 1) == 0)
                    yield return pos;
            }
        }
        public IEnumerable<IntVec3> GetTillingPositions()
        {
            this.Validate();
            foreach (var pos in this.CachedTilling)
                yield return pos;
        }

        protected override void Validate()
        {
            if (!this._dirty)
                return;
            this._dirty = false;
            this.CachedTilling.Clear();
            this.CachedSowing.Clear();
            foreach (var pos in this.Cells)
            {
                var cell = this.Town.Map.GetCell(pos);
                var block = cell.Block;
                var cellData = cell.BlockData;
                var entitiesOnBlock = this.Town.Map.GetEntitiesAt(pos.Above);
                var hasPlantAbove = entitiesOnBlock.Any(o => o.IsPlant());
                if (hasPlantAbove)
                    continue;
                if (block == BlockDefOf.Farmland.Worker)
                {
                    if (BlockFarmland.IsSeeded(cellData))
                        continue;

                    this.CachedSowing.Add(pos);
                }
                else if (cell.Material == MaterialDefOf.Soil)
                {
                    this.CachedTilling.Add(pos);
                }
            }
        }
        
        public void GetContextActions(GameObject playerEntity, ContextArgs a) { }

        public static bool IsValidFarmPosition(MapBase map, Vector3 arg)
        {
            return
                Block.GetBlockMaterial(map, arg) == MaterialDefOf.Soil
                && map.GetBlock(arg + Vector3.UnitZ) == BlockDefOf.Air.Worker;
        }

        internal IEnumerable<Entity> GetHarvestablePlantsLazy()
        {
            foreach (var pos in this.Cells)
            {
                var above = pos.Above;
                var plants = this.Town.Map.GetEntitiesAt(above);
                foreach (var p in plants)
                    if (p.TryGetComponent<PlantComponent>(out var comp) && comp.IsHarvestable)
                        yield return p;
                
            }
        }
        internal IEnumerable<GameObject> GetHarvestablePlants()
        {
            return this.Town.Map.GetObjects(this.Cells.Select(pos => (Vector3)pos.Above)).OfType<Plant>().Where(p => p.IsHarvestable);
        }
        internal IEnumerable<GameObject> GetChoppableTrees()
        {
            return this.Town.Map.GetObjects(this.Cells.Select(pos => (Vector3)pos.Above)).Where(TreeComponent.IsGrown);
        }
        public bool IsValidSeed(GameObject item)
        {
            return this.Plant is not null && item.Profile == this.Plant;
        }
        //public void AddHarvestable(Entity entity) => this._plantsHarvestable.Add(entity);
        //public void RemoveHarvestable(Entity entity) => this._plantsHarvestable.Remove(entity);
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
                    new ComboBoxNewNew<PlantSpeciesDef>(Def.GetDefs<PlantSpeciesDef>(), 128, $"Plant: ", d => $"{d?.Label ?? ""}", () => growzone?.Plant, p => Packets.SendPlant(growzone, p)),
                    new CheckBoxNew("Tilling", () => Packets.ToggleTilling(growzone), () => growzone.Tilling),
                    new CheckBoxNew("Planting", () => Packets.TogglePlanting(growzone), () => growzone.Planting),
                    new CheckBoxNew("Harvesting", () => Packets.ToggleHarvesting(growzone), () => growzone.Harvesting)
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
            return obj.Profile == this.Plant;
        }
    }
}
