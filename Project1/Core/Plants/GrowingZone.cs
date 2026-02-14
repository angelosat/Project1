using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Components.Plants;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.Simulation;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Plants
{
    public class GrowingZone : Zone, IContextable, ISelectable
    {
        
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
        public GrowingZone() { }
        public GrowingZone(ZoneManager manager) : base(manager) { }
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
        internal bool IsValidTilling(IntVec3 global) => this.CachedTilling.Contains(global);
        
        internal bool IsValidPlanting(IntVec3 global) => this.CachedSowing.Contains(global);
        
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
                    new ComboBoxNewNew<PlantSpeciesDef>(Def.GetDefs<PlantSpeciesDef>(), 128, $"Plant: ", d => $"{d?.LabelReadable ?? ""}", () => growzone?.Plant, p => PacketsGrowingZones.SendPlant(growzone, p)),
                    new CheckBoxNew("Tilling", () => PacketsGrowingZones.ToggleTilling(growzone), () => growzone.Tilling),
                    new CheckBoxNew("Planting", () => PacketsGrowingZones.TogglePlanting(growzone), () => growzone.Planting),
                    new CheckBoxNew("Harvesting", () => PacketsGrowingZones.ToggleHarvesting(growzone), () => growzone.Harvesting)
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
