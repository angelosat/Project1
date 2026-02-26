using Project1.Core.Blocks;
using Project1.Core.Input;
using Project1.Core.Legacy.Crafting.Blocks;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
using Project1.Core.UI;
using Project1.Core.UI.Blocks;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Constructions
{
    public class ConstructionsManager : TownComponent
    {
        public static readonly QuickButton IconCancel = new QuickButton(Icon.X, KeyBind.Cancel) { HoverText = "Cancel designation" };
        public override string Name => "Constructions";
        static readonly Lazy<GuiConstructionsBrowser> WindowBuild = new();
        static readonly IHotkey HotkeyBuild;
        readonly Dictionary<IntVec3, ConstructionParams> PendingDesignations = [];
        readonly HashSet<IntVec3> DesignationLocations = [];
        readonly HashSet<BlockConstructionComp> DesignationEntities = [];
        readonly Dictionary<bool, HashSet<BlockConstructionComp>> _snapshotByReadiness = new() { { false, new() }, { true, new() } };
        internal override void ResolveReferences()
        {
            foreach (var blockentity in this.Map.BlockEntities)
            {
                if (blockentity.Comps.TryGetComp<BlockConstructionComp>(out var comp))
                {
                    this.DesignationEntities.Add(comp);
                    foreach (var pos in blockentity.CellsOccupied)
                        this.DesignationLocations.Add(pos);
                }
            }
        }
        static ConstructionsManager()
        {
            HotkeyBuild = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Build", ToggleConstructionWindow, System.Windows.Forms.Keys.B);
        }
        private static void ToggleConstructionWindow()
        {
            WindowBuild.Value.ToggleSmart();
        }
        public ConstructionsManager(Town town)
        {
            this.Town = town;
            this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(this.OnBlocksChanged);
            this.Town.Map.Events.ListenTo<ConstructionReadyEvent>(this.OnConstructionReady);
            this.Town.Map.Events.ListenTo<ConstructionFinishedEvent>(this.OnConstructionFinished);
            this.Town.Map.Events.ListenTo<BlockEntityAddedEvent>(this.OnBlockEntityAdded);
            this.Town.Map.Events.ListenTo<BlockEntityRemovedEvent>(this.OnBlockEntityRemoved);
        }

        private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (!e.Entity.HasComp<BlockConstructionComp>())
                return;
            this.Town.DesignationManager.RemoveCells(e.Entity.CellsOccupied);
        }

        private void OnBlockEntityAdded(BlockEntityAddedEvent e)
        {
            if (!e.Entity.HasComp<BlockConstructionComp>())
                return;
            //this.Town.DesignationManager.AddCells(DesignationDefOf.Construct, e.Entity.CellsOccupied, false);
            this.Town.DesignationManager.AddBlockEntities(DesignationDefOf.Construct, [e.Entity], false);
        }

        private void OnConstructionFinished(ConstructionFinishedEvent e)
        {
            this.RemoveDesignatedEntity(e.Source);
            this._dirty = true;
        }
        private void OnConstructionReady(ConstructionReadyEvent e)
        {
            if (!this.DesignationEntities.Contains(e.Source))
                throw new KeyNotFoundException($"Received {nameof(ConstructionReadyEvent)} for non-registered construction designation");
            this._dirty = true;
        }
        bool _dirty;
        void OnBlocksChanged(CellsInvalidatedEvent e)
        {
            foreach (var pos in e.Positions)
            {

                if (!this._dirty)
                    foreach (var n in pos.GetAdjacentLazy())
                    {
                        if (!this.Map.IsInBounds(n))
                            continue;
                        var entity = this.Map.GetBlockEntity(n);
                        if (entity != null && 
                            entity.Comps.TryGetComp<BlockConstructionComp>(out var comp) 
                            && this.DesignationEntities.Contains(comp))
                            this._dirty = true;
                    }
            }
        }
      
        internal HashSet<BlockConstructionComp> GetConstructionsReady()
        {
            if (this._snapshotByReadiness is null || this._dirty)
                this.CacheReadiness();
            return this._snapshotByReadiness[true];
        }
        internal HashSet<BlockConstructionComp> GetConstructionsUnready()
        {
            if (this._snapshotByReadiness is null || this._dirty)
                this.CacheReadiness();
            return this._snapshotByReadiness[false];
        }
        internal IEnumerable<IntVec3> GetAllBuildableCurrently()
        {
            return this.DesignationLocations.Where(this.IsSupported);
        }
        internal IEnumerable<BlockConstructionComp> GetAllBuildableEntities()
        {
            return this.DesignationEntities.Where(e => e.Parent.CellsOccupied.All(this.IsSupported));
        }
        internal IEnumerable<BlockConstructionComp> GetReadyForConstruction()
        {
            return this.DesignationEntities.Where(e => e.IsReady && e.Parent.CellsOccupied.All(this.IsSupported));
        }
        void CacheReadiness()
        {
            // TODO incremental tracking
            _snapshotByReadiness[false].Clear();
            _snapshotByReadiness[true].Clear();
            foreach(var e in this.DesignationEntities)
            {
                if (e.IsReady && e.Parent.CellsOccupied.Any(this.IsSupported))
                    _snapshotByReadiness[true].Add(e);
                else
                    _snapshotByReadiness[false].Add(e);
            }
        }

        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Build [{HotkeyBuild.GetLabel()}]", () => WindowBuild.Value.Toggle());
        }
        internal bool IsDesignatedConstruction(IntVec3 vector3)
        {
            return this.DesignationLocations.Contains(vector3);
        }
        internal bool IsDesignatedConstruction(BlockConstructionComp comp)
        {
            return this.DesignationEntities.Contains(comp);
        }
        internal bool IsSupported(IntVec3 global)
        {
            if (!this.IsDesignatedConstruction(global))
                return false;
            return this.Map.IsAdjacentToSolid(global);
        }
        internal override void UpdateOrderButtons()
        {
            var selected = SelectionManager.Instance.CurrentSelections;
            var selectedType = selected.First().GetType();
            var selectedBlockEntities = selected.OfType<BlockEntity>();
            var selectedCellSelections = selected.OfType<CellSelection>();

            if (selectedType == typeof(BlockEntity))
            {
                var constructionTargets =
                    selectedBlockEntities.Where(s => s.HasComp<BlockConstructionComp>());
                if (!constructionTargets.Any())
                    return;
                SelectionManager.AddOrderButton(IconCancel, cancelNew, constructionTargets);
                static void cancelNew(List<ISelectable> targets) =>
                    Ingame.Instance.Events.Post(new PlayerCancelledConstructionEvent([.. targets.Select(t => (IntVec3)t.Global)]));
            }
            else if (selectedType == typeof(CellSelection))
            {
                var filteredTargets = selected.Where(t => this.Map.GetBlockEntity(t.Global) is BlockEntity b && b.HasComp<BlockConstructionComp>());
                if (!filteredTargets.Any())
                    return;
                SelectionManager.AddOrderButton(IconCancel, cancelNew, filteredTargets);
                static void cancelNew(List<ISelectable> targets) =>
                    Ingame.Instance.Events.Post(new PlayerCancelledConstructionEvent([.. targets.Select(t => (IntVec3)t.Global)]));
            }
        }
        internal void Designate(IEnumerable<IntVec3> positions, ConstructionDesignationArgs args, bool removing)
        {
            var map = this.Town.Map;

            if (removing)
            {
                RemoveNew(positions);
                return;
            }
            foreach (var pos in positions)
            {
                if (!map.IsValidBuildSpot(pos))
                    continue;
                this.PlaceDesignation(pos, args);
            }
        }
        public void PlaceDesignation(IntVec3 global, ConstructionDesignationArgs args)
        {
            var map = this.Map;

            var entity = BlockDefOf.Designation.CreateEntity(global);
            var comp = entity.GetComp<BlockConstructionComp>();
            this.DesignationEntities.Add(comp);
            var footprint = args.Block.Block.GetFootprint(map, global, args.Orientation);
            foreach (var cell in footprint)
                entity.CellsOccupied.Add(cell.global);
            comp.SetArgs(args);
            foreach (var pos in entity.CellsOccupied)
            {
                map.GetChunk(pos).InvalidateSlice(pos.Z);
                this.DesignationLocations.Add(pos);
            }
            //map.AddBlockEntityInternal(entity);
            var mapedit = new MapEdit(this.Map);
            mapedit.AddEntity(entity);
            mapedit.Flush();
            this._dirty = true;
        }
        
        internal bool RemoveNew(IEnumerable<IntVec3> positions)
        {
            var map = this.Town.Map;
            var snapshot = positions.ToHashSet();
            var comps = this.DesignationEntities.Where(c => c.Parent.CellsOccupied.Any(snapshot.Contains));
            foreach(var comp in comps)
                this.RemoveDesignatedEntity(comp);
            return true;
        }
        private void RemoveDesignatedEntity(BlockConstructionComp comp)
        {
            var entity = comp.Parent;
            this.DesignationEntities.Remove(comp);
            foreach (var child in entity.CellsOccupied)
            {
                this.DesignationLocations.Remove(child);
                this.Map.GetChunk(child).InvalidateSlice(child.Z);
            }
            var mapedit = new MapEdit(this.Map);
            mapedit.RemoveEntity(entity);
            mapedit.Flush();
        }
        GroupBox _pendingDesignationLabel;
        GroupBox PendingDesignationLabel => this._pendingDesignationLabel ??= new GroupBox();
        GroupBox UpdatePendingDesignationLabel(ConstructionParams pending)
        {
            this.PendingDesignationLabel.ClearControls();
            this.PendingDesignationLabel.AddControlsLineWrap(Label.ParseNewNew("Pending Construction: ", pending));
            return this.PendingDesignationLabel;
        }

        internal override void OnTargetSelected(IUISelection info, TargetArgs targetArgs)
        {
            var global = (IntVec3)targetArgs.Global;
            if (this.PendingDesignations.TryGetValue(global, out var pending))
            {
                info.AddInfo(this.UpdatePendingDesignationLabel(pending));
            }
        }
        class ConstructionParams : Inspectable, ISaveable, ISerializableNew<ConstructionParams>
        {
            public IntVec3 Global;
            public int Orientation;
            public ProductMaterialPair Product;
            public override string LabelReadable => this.Product.Block.LabelReadable;

            public ConstructionParams()
            {

            }
            public ConstructionParams(IntVec3 global, int orientation, ProductMaterialPair product)
            {
                this.Global = global;
                this.Orientation = orientation;
                this.Product = product;
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.Global.Save(tag, "Global");
                this.Orientation.Save(tag, "Orientation");
                this.Product.Save(tag, "Product");
                return tag;
            }
            public ISaveable Load(SaveTag tag)
            {
                this.Global = tag.LoadIntVec3("Global");
                this.Product = new ProductMaterialPair(tag["Product"]);
                this.Orientation = tag.GetValue<int>("Orientation");
                return this;
            }

            public void Write(IDataWriter w)
            {
                w.Write(this.Global);
                w.Write(this.Orientation);
                this.Product.Write(w);
            }
            public ConstructionParams Read(IDataReader r)
            {
                this.Global = r.ReadIntVec3();
                this.Orientation = r.ReadInt32();
                this.Product = new(r);
                return this;
            }

            public static ConstructionParams Create(IDataReader r) => new ConstructionParams().Read(r);
        }
    }
}