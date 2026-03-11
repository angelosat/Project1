using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Plants
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
                if (block == BlockDefOf.Farmland.Block)
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
        public override IEnumerable<(string label, Type type)> GetSelectionTabs()
        {
            yield return ("Plant", typeof(GrowZoneGui));
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
                    //if (t.Type != TargetType.Cell)
                    //    return;
                    if (t is not CellSelection cell)
                        return;
                    if (cell.Map.Town.ZoneManager.GetZoneAt<GrowingZone>(cell.Global) is not GrowingZone gz)
                        return;
                    win.GetData(gz);
                });
                return box.Window;
            }
        }
        class GrowZoneGui : GroupBox, ISelectionBound
        {
            GrowingZone CurrentGrowZone;
            public ISelectable CurrentSelection { get => this.CurrentGrowZone; set => this.CurrentGrowZone = value as GrowingZone; }
            public GrowZoneGui()
            {
                var box = new GroupBox();// 300, 200);
                box.AddControlsVertically(
                    new ComboBoxNewNew<PlantSpeciesDef>(Def.GetDefs<PlantSpeciesDef>(), 128, $"Plant: ", d => $"{d?.LabelReadable ?? ""}", () => this.CurrentGrowZone?.Plant, p => PacketsGrowingZones.SendPlant(this.CurrentGrowZone, p)),
                    new CheckBoxNew("Tilling", () => PacketsGrowingZones.ToggleTilling(this.CurrentGrowZone), () => this.CurrentGrowZone.Tilling),
                    new CheckBoxNew("Planting", () => PacketsGrowingZones.TogglePlanting(this.CurrentGrowZone), () => this.CurrentGrowZone.Planting),
                    new CheckBoxNew("Harvesting", () => PacketsGrowingZones.ToggleHarvesting(this.CurrentGrowZone), () => this.CurrentGrowZone.Harvesting)
                    );

            }
            public void OnBind(ISelectable selectable)
            {
                throw new NotImplementedException();
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
