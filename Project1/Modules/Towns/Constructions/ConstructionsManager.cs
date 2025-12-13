using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class ConstructionsManager : TownComponent
    {
        public static readonly QuickButton IconCancel = new QuickButton(UI.Icon.X, KeyBind.Cancel) { HoverText = "Cancel designation" };

        public override string Name => "Constructions";

        static readonly Lazy<GuiConstructionsBrowser> WindowBuild = new();
        static readonly IHotkey HotkeyBuild;
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
            this.Town.Map.World.Events.ListenTo<BlocksUpdatedEvent>(this.HandleBlocksChanged);
        }
        readonly Dictionary<IntVec3, ConstructionParams> PendingDesignations = new();
        readonly HashSet<IntVec3> Designations = new();

        internal IEnumerable<IntVec3> GetAllBuildableCurrently()
        {
            return this.Designations.Where(this.IsBuildableCurrently);
        }

        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Build [{HotkeyBuild.GetLabel()}]", () => WindowBuild.Value.Toggle());
        }

        public override void Write(IDataWriter w)
        {
            this.Designations.Write(w);
            this.PendingDesignations.Values.WriteNew(w);
        }
        public override void Read(IDataReader r)
        {
            this.Designations.Read(r);
            this.PendingDesignations.Read(r, i => i.Global);
        }

        protected override void AddSaveData(SaveTag tag)
        {
            this.Designations.Save(tag, "Designations");
            this.PendingDesignations.Values.SaveNewBEST(tag, "PendingDesignations");
        }
        public override void Load(SaveTag tag)
        {
            this.Designations.Load(tag, "Designations");
            this.PendingDesignations.Load(tag, "PendingDesignations", i => i.Global);
        }

        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        /// I ACTUALLY NEED IT TO ADD PENDING DESIGNATIONS
        //        case Components.Message.Types.BlocksChanged:
        //            foreach (var pos in e.Parameters[1] as IEnumerable<IntVec3>)
        //                this.TryHandlePendingDesignation(pos);
        //            break;

        //        //case Components.Message.Types.ZoneDesignation:
        //        //    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<IntVec3>, (bool)e.Parameters[2]);
        //        //    break;

        //        default:
        //            break;
        //    }
        //}
        void HandleBlocksChanged(BlocksUpdatedEvent e)
        {
            foreach (var pos in e.Positions)
                this.TryHandlePendingDesignation(pos);
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
        bool TryHandlePendingDesignation(IntVec3 global)
        {
            var map = this.Map;
            var block = map.GetBlock(global);
            if (this.PendingDesignations.TryGetValue(global, out var pending))
            {
                if (block is BlockAir)
                {
                    this.PlaceDesignation(global, 0, 0, pending.Orientation, pending.Product);
                    //this.PendingDesignations.Remove(global);
                    this.RemovePendingDesignation(global);
                    return true;
                }
            }
            else if (this.Designations.Contains(global))
            {
                if (block is not BlockDesignation && block is not BlockConstruction)
                    this.Designations.Remove(global);
            }
            return false;
        }

        internal bool IsDesignatedConstruction(IntVec3 vector3)
        {
            return this.Designations.Contains(vector3);
        }
        internal bool IsBuildableCurrently(IntVec3 global)
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
            var selectedDesignations = distinctCellOrigins.Intersect(this.Designations);
            if (!selectedDesignations.Any())
                return;
            SelectionManager.AddButton(IconCancel, cancel, selectedDesignations);

            static void cancel(List<TargetArgs> positions) => PacketDesignation.Send(Client.Instance, false, positions, null);
        }
        public void Handle(ToolBlockBuild.Args args, ProductMaterialPair product, List<IntVec3> positions)
        {
            this.PlaceDesignations(args, product, positions);
        }

        private void PlaceDesignations(ToolBlockBuild.Args args, ProductMaterialPair product, List<IntVec3> positions)
        {
            var map = this.Town.Map;
            if (args.Removing)
            {
                foreach (var pos in positions)
                {
                    if (map.GetBlockEntity(pos) is BlockDesignation.BlockDesignationEntity desEntity)
                        this.Designations.Remove(desEntity.OriginGlobal);
                    else if (this.PendingDesignations.ContainsKey(pos))
                    {
                        var cell = map.GetCell(pos);
                        var existingBlockRemovalDesignation = this.DetermineBlockRemovalDesignation(cell);
                        this.Town.DesignationManager.RemoveDesignation(existingBlockRemovalDesignation, pos);
                        //this.PendingDesignations.Remove(pos);
                        this.RemovePendingDesignation(pos);
                    }
                }
                map.RemoveBlocks(positions.Where(vec => map.GetBlock(vec) == BlockDefOf.Designation), false);
            }
            else
                foreach (var pos in positions)
                {
                    if (!map.IsValidBuildSpot(pos))
                        continue;

                    if(product.Requirement is null)
                    {
                        product.Place(map, pos);
                        return;
                    }
                    var cell = map.GetCell(pos);

                    if (cell.Block == BlockDefOf.Air)
                        this.PlaceDesignation(pos, 0, 0, args.Orientation, product);
                    else if(cell.Block != BlockDefOf.Designation)
                    {
                        var existingBlockRemovalDesignation = this.DetermineBlockRemovalDesignation(cell);
                        this.Town.DesignationManager.Add(existingBlockRemovalDesignation, pos.At(map));
                        this.AddPendingDesignation(pos, args.Orientation, product);
                    }
                }
        }
       
        DesignationDef DetermineBlockRemovalDesignation(Cell cell)
        {
            if (cell.Block.IsDeconstructible)
                return DesignationDefOf.Deconstruct;
            else if (cell.Block.IsMinable)
                return DesignationDefOf.Mine;
            else
                throw new Exception();
        }
       
        public void TabGetter(Action<string, Action> getter)
        {
            throw new NotImplementedException();
        }
        public void PlaceDesignation(IntVec3 global, byte data, int variation, int orientation, ProductMaterialPair product)
        {
            var map = this.Map;
            BlockDesignation.Place(map, global, data, variation, orientation, product);
            this.Designations.Add(global);
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
            this.PendingDesignationLabel.AddControlsLineWrap(UI.Label.ParseNewNew("Pending Construction: ", pending));
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
            public override string Label => this.Product.Block.Label;

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

            //public string GetName()
            //{
            //    throw new NotImplementedException();
            //}

            //public void GetSelectionInfo(IUISelection panel)
            //{
            //    throw new NotImplementedException();
            //}

            //public IEnumerable<(string name, Action action)> GetInfoTabs()
            //{
            //    throw new NotImplementedException();
            //}

            //public void GetQuickButtons(SelectionManager panel)
            //{
            //    throw new NotImplementedException();
            //}

            //public void TabGetter(Action<string, Action> getter)
            //{
            //    throw new NotImplementedException();
            //}
        }
    }
}
