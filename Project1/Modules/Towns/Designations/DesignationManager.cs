using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class DesignationManager : TownComponent
    {
        public override string Name => "Designation Manager";

        readonly ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>> Designations;
        readonly Dictionary<DesignationDef, BlockRendererObservable> Renderers = [];

        static DesignationManager()
        {
            PacketDesignation.Init();

            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Designations", ToggleGui, System.Windows.Forms.Keys.U);

            foreach (var d in Def.GetDefs<DesignationDef>())
                HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, $"Designate: {d.Label}", delegate { SetTool(d); });
        }

        internal ObservableHashSet<TargetArgs> GetDesignations(DesignationDef des)
        {
            return this.Designations[des];
        }

        internal bool RemoveDesignation(DesignationDef des, TargetArgs target)
        {
            var removed = this.Designations[des].Remove(target);
            if (removed)
                this.UpdateQuickButtons();
            return removed;
        }
        internal bool RemoveDesignation(DesignationDef des, IntVec3 target)
        {
            return this.RemoveDesignation(des, target.At(this.Map));
        }
        public DesignationManager(Town town) : base(town)
        {
            var desDefs = Def.GetDefs<DesignationDef>();

            this.Designations = new ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>>(desDefs.ToDictionary(d => d, d => new ObservableHashSet<TargetArgs>()));
            //this.Designations = new ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>>(new Dictionary<DesignationDef, ObservableHashSet<TargetArgs>>() {
            //    { DesignationDefOf.Deconstruct, new ObservableHashSet<TargetArgs>() },
            //    { DesignationDefOf.Mine, new ObservableHashSet<TargetArgs>()},
            //    { DesignationDefOf.Switch, new ObservableHashSet<TargetArgs>()},
            //    { DesignationDefOf.Chop, new ObservableHashSet<TargetArgs>()},
            //    { DesignationDefOf.Harvest, new ObservableHashSet<TargetArgs>()}
            //           });

            //this.Renderers.Add(DesignationDefOf.Deconstruct, new(this.Designations[DesignationDefOf.Deconstruct]));
            //this.Renderers.Add(DesignationDefOf.Mine, new(this.Designations[DesignationDefOf.Mine]));
            //this.Renderers.Add(DesignationDefOf.Switch, new(this.Designations[DesignationDefOf.Switch]));

            foreach (var d in desDefs)
                if (d.AffectsBlocks)
                    this.Renderers.Add(d, new(this.Designations[d]));

            foreach (var r in this.Designations.Values)
                r.CollectionChanged += this.R_CollectionChanged;

            this.Town.Map.World.Events.ListenTo<BlocksChangedEvent>(this.HandleBlocksChanged);

        }

        private void R_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Network.CurrentNetwork != Ingame.Net)
                return;

            //var removed = e.OldItems?.Cast<IntVec3>() ?? [];
            //foreach (var pos in removed)
            //    if (SelectionManager.SingleSelectedCell == pos)
            //        SelectionManager.RemoveInfo(this.PendingDesignationLabel);

            var removed = e.OldItems?.Cast<TargetArgs>() ?? [];
            foreach (var target in removed)
            {
                if (target.Type == TargetType.Position)
                {
                    var pos = target.Global;
                    if (SelectionManager.SingleSelectedCell == pos)
                        SelectionManager.RemoveInfo(this.PendingDesignationLabel);
                }
            }
            //var added = e.NewItems?.Cast<IntVec3>() ?? Enumerable.Empty<IntVec3>();
            //foreach (var pos in added)
            //    if (SelectionManager.SingleSelectedCell == pos)
            //        SelectionManager.AddInfoNew(this.UpdatePendingDesignationLabel(this.Designations.First(d => d.Value.Contains(pos)).Key));

            var added = e.NewItems?.Cast<TargetArgs>() ?? Enumerable.Empty<TargetArgs>();
            foreach (var target in added)
                if(target.Type == TargetType.Position)
                {
                    var pos = target.Global;
                    if (SelectionManager.SingleSelectedCell == pos)
                        SelectionManager.AddInfoNew(this.UpdatePendingDesignationLabel(this.Designations.First(d => d.Value.Contains(target)).Key));
                }
        }

        internal void Add(DesignationDef designation, TargetArgs position, bool remove = false)
        {
            this.Add(designation, [position], remove);
        }
        internal void Add(DesignationDef designation, IEnumerable<TargetArgs> positions, bool remove)
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
                    //else if (designation.IsValid(this.Town.Map, pos) || this.Map.IsUndiscovered(pos))
                    else if (designation.IsValid(pos) || (pos.Type == TargetType.Position && this.Map.IsUndiscovered(pos.Global)))
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
        public DesignationDef GetDesignation(TargetArgs global)
        {
            return this.Designations.FirstOrDefault(d => d.Value.Contains(global)).Key; // will this return null if no designation?
        }
        internal bool IsDesignation(TargetArgs target)
        {
            return this.Designations.Values.Any(v => v.Contains(target));
        }
        internal bool IsDesignation(IntVec3 global)
        {
            return this.Designations.Values.Any(v => v.Contains(global.At(this.Map)));
        }
        internal bool IsDesignation(IntVec3 global, DesignationDef desType)
        {
            var contains = this.Designations[desType].Contains(global.At(this.Map));
            return contains;
        }
        internal bool IsDesignation(TargetArgs global, DesignationDef desType)
        {
            var contains = this.Designations[desType].Contains(global);
            return contains;
        }
        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Components.Message.Types.BlocksChanged:
        //            this.HandleBlocksChanged(e.Parameters[1] as IEnumerable<IntVec3>);
        //            break;

        //        //case Components.Message.Types.ZoneDesignation:
        //        //    this.Add(e.Parameters[0] as DesignationDef, e.Parameters[1] as List<TargetArgs>, (bool)e.Parameters[2]);
        //        //    break;

        //        default:
        //            break;
        //    }
        //}
        void HandleBlocksChanged(BlocksChangedEvent e)
        {
            foreach (var des in this.Designations)
            {
                foreach (var target in e.Positions)
                {
                    if (!des.Key.IsValid(this.Map, target))
                        des.Value.Remove(target.At(this.Map));
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
                //tag.TryGetTag(des.Name, v => this.Designations[des].LoadIntVecs(v));
                tag.TryGetTag(des.Name, v => this.Designations[des].LoadTargets(v));
        }
        public override void Write(IDataWriter w)
        {
            foreach (var des in this.Designations)
                w.Write(des.Value);
        }
        public override void Read(IDataReader r)
        {
            foreach (var des in this.Designations.Keys.ToList())
                //this.Designations[des].ReadIntVec3(r);
                this.Designations[des].ReadTargets(r);
        }

        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Designations [{Hotkey.GetLabel()}]", ToggleGui);
        }

        private static readonly Lazy<Control> _guiNew = new(() => ContextMenuManager.CreateContextSubMenu("Designations", GetContextSubmenuItems()).HideOnAnyClick());

        static void ToggleGui()
        {
            _guiNew.Value.Toggle();
        }
        static IEnumerable<(string, Action)> GetContextSubmenuItems()
        {
            yield return ("Remove", () => SetTool(null));
            foreach (var def in Ingame.CurrentMap.Town.DesignationManager.Designations.Keys)
                yield return (def.Label, () => SetTool(def));
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
            var selectedTargets = SelectionManager.Selected;
            var fromblockentities = selectedTargets.Select(i => this.Map.GetBlockEntity(i.Global)).OfType<BlockEntity>().Select(b => b.OriginGlobal.At(this.Town.Map));// new TargetArgs(b.OriginGlobal));
            selectedTargets = selectedTargets.Concat(fromblockentities).Distinct();

            var areTask = selectedTargets.Where(e => this.Designations.Values.Any(t => t.Contains(e)));// new TargetArgs(e))));
            foreach (var d in this.Designations) // need to handle construction designations differently because of multi-celled designations 
            {
                var selectedDesignations = d.Value.Intersect(selectedTargets);
                if (selectedDesignations.Any())
                    SelectionManager.AddButton(d.Key.IconRemove, remove, selectedDesignations);
                else
                    SelectionManager.RemoveButton(d.Key.IconRemove);
            }

            var areNotTask = selectedTargets
                .Except(areTask)
                .Where(t => this.AllDesignationDefs.Any(d => d.IsValid(t))).ToList();

            var splits = this.AllDesignationDefs.ToDictionary(d => d, d => areNotTask.FindAll(t => d.IsValid(t)));
            foreach (var s in this.AllDesignationDefs)
            {
                if (!splits.TryGetValue(s, out var list) || !list.Any())
                    SelectionManager.RemoveButton(s.IconAdd);
                else
                    SelectionManager.AddButton(s.IconAdd, targets => add(targets, s), list);
            }

            static void remove(IEnumerable<TargetArgs> targets)
            {
                PacketDesignation.Send(Client.Instance, false, targets, null);
            }
            static void add(IEnumerable<TargetArgs> targets, DesignationDef des)
            {
                PacketDesignation.Send(Client.Instance, false, targets, des);
            }
        }
        List<DesignationDef> designationDefs;
        List<DesignationDef> AllDesignationDefs => this.designationDefs ??= Def.GetDefs<DesignationDef>().ToList();//.Except(new DesignationDef[] { DesignationDefOf.Remove }).ToList();

        private static readonly IHotkey Hotkey;

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
            if (this.Designations.FirstOrDefault(d => d.Value.Contains(targetArgs)).Key is DesignationDef des)
                info.AddInfo(this.UpdatePendingDesignationLabel(des));
        }

        public override void DrawUI(SpriteBatch sb, MapBase map, Camera cam)
        {
            foreach(var entityDes in this.Designations)
            {
                if (entityDes.Key.AffectsBlocks)
                    continue;
                foreach(var entity in entityDes.Value)
                {
                    var icon = entityDes.Key.IconAdd.Icon;
                    icon.DrawFloating(sb, cam, entity);
                }
            }
        }
    }
}
