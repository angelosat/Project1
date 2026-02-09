using Project1.Core.Construction.Tools;
using Project1.Core.Construction.UI;
using Project1.Core.Towns.Designations;
using Project1.Core.Base;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Input.Tools;
using Project1.Core.Input.UI;
using Project1.Core.Interfaces;
using Project1.Core.Legacy.Crafting.Blocks;
using Project1.Core.Net;
using Project1.Core.Screens;
using Project1.Core.WorldGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Core.Input;
using Project1.Framework;

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
        Dictionary<bool, HashSet<BlockConstructionComp>> _snapshotByReadiness = new() { { false, new() }, { true, new() } };
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

        private void Add(DesignationDef designation, List<IntVec3> positions, bool remove)
        {
            if (designation is null)// == DesignationDefOf.Remove)
            {
                foreach (var pos in positions)
                {
                    if (this.Map.GetBlockEntity<BlockDesignation.BlockDesignationEntity>(pos) is BlockDesignation.BlockDesignationEntity blockEntity)
                    {
                        var origin = blockEntity.OriginGlobal;
                        this.Map.RemoveBlock(origin);
                    }
                    else if (this.PendingDesignations.ContainsKey(pos))
                        this.RemovePendingDesignation(pos);
                }
            }
        }
        void AddPendingDesignation(IntVec3 pos, int orientation, ProductMaterialPair product)
        {
            var pending = new ConstructionParams(pos, orientation, product);
            this.PendingDesignations[pos] = pending;
            if(Network.CurrentNetwork == Ingame.Net)
                if (SelectionManager.SingleSelectedCell == pos)
                    SelectionManager.AddInfoNew(UpdatePendingDesignationLabel(pending));
        }
        void RemovePendingDesignation(IntVec3 pos)
        {
            this.PendingDesignations.Remove(pos);
            if(Network.CurrentNetwork == Ingame.Net)
                if (SelectionManager.SingleSelectedCell == pos)
                    SelectionManager.RemoveInfo(this.PendingDesignationLabel);
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

        public string GetName()
        {
            return "Build";
        }

        public void GetSelectionInfo(IUISelection panel)
        {
            panel.AddInfo(new Label() { Text = "Buildings" });
        }
        internal override void UpdateQuickButtons()
        {
            var cells = SelectionManager.SelectedCells;
            var distinctCellOrigins = cells.Select(c => Cell.GetOrigin(this.Map, c)).Distinct();
            var selectedDesignations = distinctCellOrigins.Intersect(this.DesignationLocations);
            if (!selectedDesignations.Any())
                return;
            SelectionManager.AddButton(IconCancel, cancel, selectedDesignations);

            static void cancel(List<TargetArgs> positions) => PacketDesignation.Send(Client.Instance, false, positions, null);
        }
        internal void Designate(ToolBlockBuild.Args tool, ConstructionDesignationArgs args)
        {
            var map = this.Town.Map;
            var positions = tool.ToolDef.Worker.GetPositions(tool.Begin, tool.End);

            if (tool.Removing)
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
 
        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }


        public void PlaceDesignation(IntVec3 global, ConstructionDesignationArgs args)
        {
            var map = this.Map;

            var entity = BlockDefOf.Designation.CreateEntity(global);
            var comp = entity.GetComp<BlockConstructionComp>();
            this.DesignationEntities.Add(comp);
            var footprint = args.Block.Worker.GetFootprint(map, global, args.Orientation);
            foreach (var cell in footprint)
                entity.CellsOccupied.Add(cell.global);
            comp.SetArgs(args);
            foreach (var pos in entity.CellsOccupied)
            {
                map.GetChunk(pos).InvalidateSlice(pos.Z);
                this.DesignationLocations.Add(pos);
            }
            //map.AddBlockEntity(global, entity);
            //map.AddBlockEntity(entity);
            map.AddBlockEntityInternal(entity);

            this._dirty = true;
        }
        public void PlaceDesignationOld(IntVec3 global, ConstructionDesignationArgs args)
        {
            var map = this.Map;

            var entity = BlockDefOf.Designation.CreateEntity(global);
            map.AddBlockEntity(entity);
            var comp = entity.GetComp<BlockConstructionComp>();
            this.DesignationEntities.Add(comp);

            comp.SetArgs(args);
            foreach (var pos in entity.CellsOccupied)
            {
                map.GetChunk(pos).InvalidateSlice(pos.Z);
                this.DesignationLocations.Add(global);
            }
            this._dirty = true;
        }
        private void RemoveNew(IEnumerable<IntVec3> positions)
        {
            var map = this.Town.Map;
            var snapshot = positions.ToHashSet();
            var comps = this.DesignationEntities.Where(c => c.Parent.CellsOccupied.Any(snapshot.Contains));
            foreach(var comp in comps)
            {
                this.RemoveDesignatedEntity(comp);
            }
        }

        private void RemoveDesignatedEntity(BlockConstructionComp comp)
        {
            var entity = comp.Parent;
            this.DesignationEntities.Remove(comp);
            foreach (var child in entity.CellsOccupied)
            {
                this.DesignationLocations.Remove(child);
                //this.Town.DesignationManager.RemoveDesignation(DesignationDefOf.Construct, new TargetArgs(this.Map, child));
                this.Map.GetChunk(child).InvalidateSlice(child.Z);
            }
        }

        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            throw new NotImplementedException();
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