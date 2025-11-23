using Start_a_Town_;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class DesignationManager : TownComponent
    {
        public override string Name => "Designation Manager";

        //readonly ReadOnlyDictionary<DesignationDef, ObservableCollection<IntVec3>> Designations;
        readonly ReadOnlyDictionary<DesignationDef, ObservableHashSet<IntVec3>> Designations;
        readonly Dictionary<DesignationDef, BlockRendererObservable> Renderers = new();

        static DesignationManager()
        {
            PacketDesignation.Init();

            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Designations", ToggleGui, System.Windows.Forms.Keys.U);

            foreach (var d in Def.GetDefs<DesignationDef>())
                HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, $"Designate: {d.Label}", delegate { SetTool(d); });
        }

        internal ObservableHashSet<IntVec3> GetDesignations(DesignationDef des)
        {
            return this.Designations[des];
        }

        internal bool RemoveDesignation(DesignationDef des, IntVec3 global)
        {
            var removed = this.Designations[des].Remove(global);
            if (removed)
                this.UpdateQuickButtons();
            return removed;
        }

        public DesignationManager(Town town) : base(town)
        {
            //Designations = new ReadOnlyDictionary<DesignationDef, ObservableCollection<IntVec3>>(
            //    new Dictionary<DesignationDef, ObservableCollection<IntVec3>>() {
            //    { DesignationDefOf.Deconstruct, new ObservableCollection<IntVec3>() },
            //    { DesignationDefOf.Mine, new ObservableCollection<IntVec3>()},
            //    { DesignationDefOf.Switch, new ObservableCollection<IntVec3>()}
            //});
            this.Designations = new ReadOnlyDictionary<DesignationDef, ObservableHashSet<IntVec3>>(new Dictionary<DesignationDef, ObservableHashSet<IntVec3>>() {
                { DesignationDefOf.Deconstruct, new ObservableHashSet<IntVec3>() },
                { DesignationDefOf.Mine, new ObservableHashSet<IntVec3>()},
                { DesignationDefOf.Switch, new ObservableHashSet<IntVec3>()}
                       });

            this.Renderers.Add(DesignationDefOf.Deconstruct, new(this.Designations[DesignationDefOf.Deconstruct]));
            this.Renderers.Add(DesignationDefOf.Mine, new(this.Designations[DesignationDefOf.Mine]));
            this.Renderers.Add(DesignationDefOf.Switch, new(this.Designations[DesignationDefOf.Switch]));

            foreach (var r in this.Designations.Values)
                r.CollectionChanged += this.R_CollectionChanged;
        }

        private void R_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Network.CurrentNetwork != Ingame.Net)
                return;

            var removed = e.OldItems?.Cast<IntVec3>() ?? Enumerable.Empty<IntVec3>();
            foreach (var pos in removed)
                if (SelectionManager.SingleSelectedCell == pos)
                    SelectionManager.RemoveInfo(this.PendingDesignationLabel);

            var added = e.NewItems?.Cast<IntVec3>() ?? Enumerable.Empty<IntVec3>();
            foreach (var pos in added)
                if (SelectionManager.SingleSelectedCell == pos)
                    SelectionManager.AddInfoNew(this.UpdatePendingDesignationLabel(this.Designations.First(d => d.Value.Contains(pos)).Key));
        }

        internal void Add(DesignationDef designation, IntVec3 position, bool remove = false)
        {
            this.Add(designation, new List<IntVec3>(1) { position }, remove);
        }
        internal void Add(DesignationDef designation, List<IntVec3> positions, bool remove)
        {
            if (designation is null)
            {
                foreach (var l in this.Designations)
                    foreach (var p in positions)
                        l.Value.Remove(p);
            }
            else
            {
                var list = this.Designations[designation];
                foreach (var pos in positions)
                {
                    if (remove)
                        list.Remove(pos);
                    else if (designation.IsValid(this.Town.Map, pos) || this.Map.IsUndiscovered(pos))
                        list.Add(pos);
                }
            }
            this.UpdateQuickButtons();
        }

        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            foreach (var r in this.Renderers)
                r.Value.DrawBlocks(map, cam);
        }
        public DesignationDef GetDesignation(IntVec3 global)
        {
            return this.Designations.FirstOrDefault(d => d.Value.Contains(global)).Key; // will this return null if no designation?
        }
        internal bool IsDesignation(IntVec3 global)
        {
            return this.Designations.Values.Any(v => v.Contains(global));
        }
        internal bool IsDesignation(IntVec3 global, DesignationDef desType)
        {
            var contains = this.Designations[desType].Contains(global);
            return contains;
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                    this.HandleBlocksChanged(e.Parameters[1] as IEnumerable<IntVec3>);
                    break;

                case Components.Message.Types.ZoneDesignation:
                    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<IntVec3>, (bool)e.Parameters[2]);
                    break;

                default:
                    break;
            }
        }

        private void HandleBlocksChanged(IEnumerable<IntVec3> globals)
        {
            foreach (var des in this.Designations)
            {
                foreach (var global in globals)
                {
                    if (!des.Key.IsValid(this.Map, global))
                        des.Value.Remove(global);
                }
            }
        }

        protected override void AddSaveData(SaveTag tag)
        {
            foreach (var des in this.Designations)
                tag.Add(des.Value.ToList().Save(des.Key.Name));
        }
        public override void Load(SaveTag tag)
        {
            foreach (var des in this.Designations.Keys.ToList())
                tag.TryGetTag(des.Name, v => this.Designations[des].LoadIntVecs(v));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            foreach (var des in this.Designations)
                w.Write(des.Value);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            foreach (var des in this.Designations.Keys.ToList())
                this.Designations[des].ReadIntVec3(r);
        }

        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Designations [{Hotkey.GetLabel()}]", ToggleGui);
        }
        static Window _gui;

        public static void ToggleGui()
        {
            if (_gui is null)
            {
                var box = new ListBoxNoScroll<DesignationDef, Button>(createButton, 0).AddItems(Ingame.CurrentMap.Town.DesignationManager.Designations.Keys.Prepend(null));// DesignationDef.Remove));
                box.BackgroundColor = Microsoft.Xna.Framework.Color.Black * .5f;
                _gui = box.ToWindow("Designations").Transparent();
                _gui.Location = Controller.Instance.MouseLocation;
            }
            _gui.Toggle();

            Button createButton(DesignationDef d)
            {
                var btn = new Button(d?.Label ?? "Remove", () => SetTool(d), 96) { Tag = d };
                btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolDigging tool && btn.Tag == tool.DesignationDef;
                return btn;
            }
        }

        private static void SetTool(DesignationDef d)
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, r, a, b, d)) { DesignationDef = d });
        }

        static void Cancel()
        {
            ToolManager.SetTool(new ToolDigging((a, b, r) => PacketDesignation.Send(Client.Instance, r, a, b, null)));
        }
        internal override void UpdateQuickButtons()
        {
            if (this.Town.Net is Server)
                return;
            var selectedCells = SelectionManager.GetSelectedCells();
            var fromblockentities = selectedCells.Select(this.Map.GetBlockEntity).OfType<BlockEntity>().Select(b => b.OriginGlobal);
            selectedCells = selectedCells.Concat(fromblockentities).Distinct();

            var areTask = selectedCells.Where(e => this.Designations.Values.Any(t => t.Contains(e)));
            foreach (var d in this.Designations) // need to handle construction designations differently because of multi-celled designations 
            {
                var selectedDesignations = d.Value.Intersect(selectedCells);
                if (selectedDesignations.Any())
                    SelectionManager.AddButton(d.Key.IconRemove, cancel, selectedDesignations.Select(i => new TargetArgs(this.Map, i)));
                else
                    SelectionManager.RemoveButton(d.Key.IconRemove);
            }

            var areNotTask = selectedCells.Except(areTask).Where(t =>
                this.AllDesignationDefs.Any(d => d.IsValid(this.Town.Map, t))).ToList();

            var splits = this.AllDesignationDefs.ToDictionary(d => d, d => areNotTask.FindAll(t => d.IsValid(this.Map, t)));
            foreach (var s in this.AllDesignationDefs)
            {
                if (!splits.TryGetValue(s, out var list) || !list.Any())
                    SelectionManager.RemoveButton(s.IconAdd);
                else
                    SelectionManager.AddButton(s.IconAdd, targets => MineAdd(targets, s), list.Select(i => new TargetArgs(this.Map, i)));
            }

            static void cancel(List<TargetArgs> positions)
            {
                PacketDesignation.Send(Client.Instance, false, positions, null);
            }
        }
        List<DesignationDef> designationDefs;
        List<DesignationDef> AllDesignationDefs => this.designationDefs ??= Def.GetDefs<DesignationDef>().ToList();//.Except(new DesignationDef[] { DesignationDefOf.Remove }).ToList();

        public static readonly Icon MineIcon = new(ItemContent.PickaxeFull);
        private static readonly IHotkey Hotkey;

        static void MineAdd(List<TargetArgs> targets, DesignationDef des)
        {
            PacketDesignation.Send(Client.Instance, false, targets, des);
        }

        GroupBox _pendingDesignationLabel;
        GroupBox PendingDesignationLabel => this._pendingDesignationLabel ??= new GroupBox();
        GroupBox UpdatePendingDesignationLabel(DesignationDef des)
        {
            this.PendingDesignationLabel.ClearControls();
            this.PendingDesignationLabel.AddControlsLineWrap(UI.Label.ParseNewNew("Designation: ", des));// ( new Label(des));
            return this.PendingDesignationLabel;
        }
        internal override void OnTargetSelected(IUISelection info, TargetArgs targetArgs)
        {
            var pos = (IntVec3)targetArgs.Global;
            if (this.Designations.FirstOrDefault(d => d.Value.Contains(pos)).Key is DesignationDef des)
                info.AddInfo(this.UpdatePendingDesignationLabel(des));
        }
    }
}
