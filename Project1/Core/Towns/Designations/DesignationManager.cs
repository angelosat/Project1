using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Input.CellRendering;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Serialization;
using Project1.Core.Simulation;
using Project1.Core.Towns.Digging;
using Project1.Core.UI;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Towns.Designations
{
    [EnsureStaticCtorCall]
    public class DesignationManager : TownComponent
    {
        public override string Name => "Designation Manager";
        readonly ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>> Designations;
        readonly ReadOnlyDictionary<DesignationDef, ObservableHashSet<IntVec3>> CellDesignations;
        readonly ReadOnlyDictionary<DesignationDef, ObservableHashSet<Entity>> EntityDesignations;
        readonly ReadOnlyDictionary<DesignationDef, ObservableHashSet<BlockEntity>> BlockEntityDesignations;
        public readonly Dictionary<DesignationDef, BlockRendererObservable> Renderers = [];
        static List<DesignationDef> designationDefs;
        static List<DesignationDef> AllDesignationDefs => designationDefs ??= [.. Def.GetDefs<DesignationDef>()];
        private static readonly IHotkey Hotkey;
        GroupBox _pendingDesignationLabel;
        GroupBox PendingDesignationLabel => this._pendingDesignationLabel ??= new GroupBox();
        static DesignationManager()
        {
            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Designations", ToggleGui, System.Windows.Forms.Keys.U);

            foreach (var d in Def.GetDefs<DesignationDef>())
                HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, $"Designate: {d.LabelReadable}", delegate { SetTool(d); });
        }
        
        public DesignationManager(Town town) : base(town)
        {
            var desDefs = Def.GetDefs<DesignationDef>();

            var cellDesignationDefs = desDefs.Where(d => d.TargetType == TargetType.Cell);
            var entityDesignationDefs = desDefs.Where(d => d.TargetType == TargetType.Entity);
            var blockEntityDesignationDefs = desDefs.Where(d => d.TargetType == TargetType.BlockEntity);
            this.CellDesignations = new(cellDesignationDefs.ToDictionary(d => d, d => new ObservableHashSet<IntVec3>()));
            this.EntityDesignations = new(entityDesignationDefs.ToDictionary(d => d, d => new ObservableHashSet<Entity>()));
            this.BlockEntityDesignations = new(blockEntityDesignationDefs.ToDictionary(d => d, d => new ObservableHashSet<BlockEntity>()));

            this.Designations = new ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>>(desDefs.ToDictionary(d => d, d => new ObservableHashSet<TargetArgs>()));

            foreach (var d in desDefs)
            {
                if (d.TargetType == TargetType.Cell)
                    this.Renderers.Add(d, new(this.CellDesignations[d]));
            }

            foreach (var r in this.Designations.Values)
                r.CollectionChanged += this.R_CollectionChanged;

            this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(this.OnBlocksChanged);
            this.Town.Map.Events.ListenTo<EntityDespawnedEvent>(this.OnEntityDespawn);
        }
        private void R_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Network.CurrentNetwork != Ingame.Net)
                return;

            var removed = e.OldItems?.Cast<TargetArgs>() ?? [];
            foreach (var target in removed)
            {
                if (target.Type == TargetType.Cell)
                {
                    var pos = target.Global;
                    if (SelectionManager.SingleSelectedCell == pos)
                        SelectionManager.RemoveInfo(this.PendingDesignationLabel);
                }
            }

            var added = e.NewItems?.Cast<TargetArgs>() ?? Enumerable.Empty<TargetArgs>();
            foreach (var target in added)
                if(target.Type == TargetType.Cell)
                {
                    var pos = target.Global;
                    if (SelectionManager.SingleSelectedCell == pos)
                        SelectionManager.AddInfoNew(this.UpdatePendingDesignationLabel(this.Designations.First(d => d.Value.Contains(target)).Key));
                }
        }
        internal ObservableHashSet<TargetArgs> GetDesignations(DesignationDef des)
        {
            return this.Designations[des];
        }
        internal bool RemoveDesignation(DesignationDef des, TargetArgs target)
        {
            var removed = this.Designations[des].Remove(target);
            if (removed)
                this.UpdateOrderButtons();
            return removed;
        }
        internal bool RemoveDesignation(DesignationDef des, IntVec3 target)
        {
            return this.RemoveDesignation(des, target.At(this.Map));
        }
        internal void Remove(IEnumerable<ISelectable> targets)
        {
            foreach (var item in targets)
            {
                if (item is CellSelection cell)
                    foreach (var l in this.CellDesignations.Where(d => d.Key.IsManual))
                        l.Value.Remove(item.Global);
                else if (item is Entity entity)
                    foreach (var l in this.EntityDesignations.Where(d => d.Key.IsManual))
                        l.Value.Remove(entity);
                else if(item is BlockEntity bEntity)
                    foreach (var l in this.BlockEntityDesignations.Where(d => d.Key.IsManual))
                        l.Value.Remove(bEntity);
            }
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        internal void Edit(DesignationDef def, IntVec3 begin, IntVec3 end, bool isRemoval)
        {
            var cells = IntVec3Helper.GetBox(begin, end).Select(c => new CellSelection(this.Map, c) as ISelectable);
            if (isRemoval)
                this.RemoveCells(cells);
            else
                this.AddCells(def, cells, isRemoval);
        }
        internal void Edit(DesignationDef def, IEnumerable<ISelectable> entities, bool isRemoval)
        {
            if (!entities.Any())
                return;
            if (isRemoval)
                this.RemoveEntities(entities);
            else
                this.AddEntities(def, entities, isRemoval);
        }
        internal void Add(DesignationDef designation, IEnumerable<ISelectable> targets, bool isRemoval)
        {
            ArgumentNullException.ThrowIfNull(designation, $"Use {this.Remove} for generic designation removal instead of passing a null desigation def");
            switch (designation.TargetType)
            {
                case TargetType.Cell:
                    this.AddCells(designation, targets, isRemoval);
                    break;

                case TargetType.Entity:
                    foreach (var entity in targets.OfType<Entity>())
                    {
                        if (isRemoval && designation.IsManual)
                            this.EntityDesignations[designation].Remove(entity);
                        else if (designation.Worker.IsValid(entity))
                            this.EntityDesignations[designation].Add(entity);
                    }
                    this.Map.Events.Post(new DesignationsChangedEvent(targets));

                    break;

                case TargetType.BlockEntity:
                    foreach (var bEntity in targets.OfType<BlockEntity>())
                    {
                        if (isRemoval && designation.IsManual)
                            this.BlockEntityDesignations[designation].Remove(bEntity);
                        else if (designation.Worker.IsValid(bEntity))
                            this.BlockEntityDesignations[designation].Add(bEntity);
                    }
                    this.Map.Events.Post(new DesignationsChangedEvent(targets));

                    break;

                default:
                    throw new UnreachableException();
            }
        }
        internal void RemoveCells(IEnumerable<ISelectable> targets)
        {
            if (!targets.Any())
                return;
            foreach (var cell in targets.OfType<CellSelection>())
                foreach(var des in this.CellDesignations.Where(vk=>vk.Key.IsManual))
                    des.Value.Remove(cell.Global);
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        internal void AddCells(DesignationDef designation, IEnumerable<ISelectable> targets, bool isRemoval)
        {
            var cells = targets.OfType<CellSelection>();
            if (!cells.Any())
                return;
            var removing = isRemoval && designation.IsManual;
            var list = this.CellDesignations[designation];
            if (removing)
                foreach (var cell in cells)
                    list.Remove(cell.Global);
            else
            {
                foreach (var cell in cells)
                    if (designation.Worker.IsValid(cell) || this.Map.IsUndiscovered(cell.Global))
                        list.Add(cell.Global);
            }
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        internal void RemoveEntities(IEnumerable<ISelectable> targets)
        {
            if (!targets.Any())
                return;
            foreach (var entity in targets.OfType<Entity>())
                foreach (var des in this.EntityDesignations.Where(vk => vk.Key.IsManual))
                    des.Value.Remove(entity);
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        internal void AddEntities(DesignationDef designation, IEnumerable<ISelectable> targets, bool isRemoval)
        {
            var entities = targets.OfType<Entity>();
            if (!entities.Any())
                return;
            var removing = isRemoval && designation.IsManual;
            var list = this.EntityDesignations[designation];
            if (removing)
                foreach (var item in entities)
                    list.Remove(item);
            else
            {
                foreach (var item in entities)
                    if (designation.Worker.IsValid(item))
                        list.Add(item);
            }
            //foreach (var entity in entities.OfType<Entity>())
            //{
            //    if (isRemoval && designation.IsManual)
            //        .Remove(entity);
            //    else if (designation.Worker.IsValid(entity))
            //        this.EntityDesignations[designation].Add(entity);
            //}
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        internal void Add(DesignationDef designation, IEnumerable<IntVec3> cells, bool isRemoval)
        {
            if (designation.TargetType != TargetType.Cell)
                throw new InvalidOperationException($"Cells designation invalid for {designation}");
            this.Add(designation, cells.Select(c => new TargetArgs(this.Map, c)), isRemoval);
        }
        internal void Add(DesignationDef designation, IEnumerable<TargetArgs> positions, bool isRemoval)
        {
            if (designation is null)
            {
                foreach (var l in this.Designations.Where(d => d.Key.IsManual))
                    foreach (var p in positions)
                        l.Value.Remove(p);
            }
            else
            {
                var list = this.Designations[designation];
                foreach (var pos in positions)
                {
                    if (isRemoval && designation.IsManual)
                        list.Remove(pos);
                    else if (designation.IsValid(pos) || (pos.Type == TargetType.Cell && this.Map.IsUndiscovered(pos.Global)))
                        list.Add(pos);
                }
            }
         
            this.UpdateOrderButtons();
        }
        public DesignationDef GetDesignation(CellSelection cell)
        {
            foreach (var d in this.CellDesignations)
                if (d.Value.Contains(cell.Global))
                    return d.Key;
            return null;
        }
        public DesignationDef GetDesignation(TargetArgs global)
        {
            return this.Designations.FirstOrDefault(d => d.Value.Contains(global)).Key; // will this return null if no designation?
        }
        internal bool IsDesignation(ISelectable target)
        {
            return target switch
            {
                CellSelection => this.CellDesignations.Values.Any(v => v.Contains(target.Global)),
                Entity => this.EntityDesignations.Values.Any(v => v.Contains(target)),
                BlockEntity => this.BlockEntityDesignations.Values.Any(v => v.Contains(target)),
                _ => false
            };
        }
        //internal bool IsDesignation(ISelectable target)
        //{
        //    return target switch
        //    {
        //        CellSelection => this.CellDesignations.Values.Any(v => v.Contains(target.Global)),
        //        Entity => this.EntityDesignations.Values.Any(v => v.Contains(target)),
        //        BlockEntity => this.BlockEntityDesignations.Values.Any(v => v.Contains(target))
        //        _ => throw new UnreachableException()
        //    };
        //    //return this.Designations.Values.Any(v => v.Contains(target));
        //}
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
        void OnBlocksChanged(CellsInvalidatedEvent e)
        {
            foreach (var des in this.Designations)
            {
                foreach (var target in e.Positions)
                {
                    if (!des.Value.Contains(new TargetArgs(this.Map, target)))
                        continue;
                    if (!des.Key.IsValid(this.Map, target))
                        des.Value.Remove(target.At(this.Map));
                }
            }
        }
        void OnEntityDespawn(EntityDespawnedEvent obj)
        {
            foreach (var designations in this.Designations.Values)
                if (designations.Contains(obj.Entity))
                    designations.Remove(obj.Entity);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            foreach (var des in this.Designations)
                tag.Add(des.Value.ToList().Save(des.Key.Name));
        }
        public override void Load(SaveTag tag)
        {
            foreach (var des in this.Designations.Keys.ToList())
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
                this.Designations[des].ReadTargets(this.Map, r);
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
            //foreach (var def in Ingame.CurrentMap.Town.DesignationManager.Designations.Keys
            foreach (var def in AllDesignationDefs.Where(d => d.IsManual))
                yield return (def.LabelReadable, () => SetTool(def));
        }
        private static void SetTool(DesignationDef d)
        {
            //ToolManager.SetTool(new ToolDesignation((begin, end, isRemoval) => PacketsDesignations.Send(Client.Instance, isRemoval, begin, end, d)) { DesignationDef = d });
            ToolManager.SetTool(new ToolDesignation((begin, end, isRemoval) => Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(d, begin, end, isRemoval))));
        }
        static void Cancel()
        {
            ToolManager.SetTool(new ToolDesignation((a, b, r) => PacketsDesignations.Send(Client.Instance, r, a, b, null)));
        }
        internal override void UpdateOrderButtons()
        {
            //if (this.Town.Net is Server)
            //    return;
            return;
            //var selectedCells = SelectionManager.Instance.CurrentSelections.OfType<CellSelection>();
            //var selectedEntities = SelectionManager.Instance.CurrentSelections.OfType<Entity>();
            //var selectedBlockEntities = SelectionManager.Instance.CurrentSelections.OfType<BlockEntity>();
            ////if (!selectedCells.Any())
            ////    return;
            ////var fromblockentities = selected.Select(i => this.Map.GetBlockEntity(i.Global)).OfType<BlockEntity>().Select(b => b.OriginGlobal.At(this.Town.Map));// new TargetArgs(b.OriginGlobal));
            ////var selectedBlockEntities = selected.OfType<BlockEntity>();
            //var fromblockentities = selectedCells.Select(i => this.Map.GetBlockEntity(i.Global)).OfType<BlockEntity>().Select(b => this.Town.Map.Select(b.OriginGlobal));
            //var selectedCells = selectedCells.Union(fromblockentities);
            //var vecs = selectedCells.Select(c => c.Global);
            //var areExisting = selectedCells.Where(e => this.Designations.Values.Any(t => t.Contains(e)));// new TargetArgs(e))));

            //foreach (var (def, list) in this.Designations) // need to handle construction designations differently because of multi-celled designations 
            //{
            //    if (!def.IsManual)
            //        continue;
            //    var existingDesignations = list.Intersect(selectedCells);
            //    if (existingDesignations.Any())
            //        SelectionManager.AddOrderButton(def.IconRemove, remove, existingDesignations);
            //    else
            //        SelectionManager.RemoveOrderButton(def.IconRemove);
            //}

            //var availableDesignations = selectedCells
            //    .Except(areExisting)
            //    .Where(t => AllDesignationDefs.Any(d => d.IsValid(t))).ToList();

            //var splits = AllDesignationDefs.ToDictionary(d => d, d => availableDesignations.FindAll(t => d.IsValid(t)));
            //foreach (var s in AllDesignationDefs)
            //{
            //    if (!s.IsManual)
            //        continue;
            //    if (!splits.TryGetValue(s, out var list) || list.Count == 0)
            //        SelectionManager.RemoveOrderButton(s.IconAdd);
            //    else
            //        SelectionManager.AddOrderButton(s.IconAdd, targets => add(targets, s), list);
            //}

            //void remove(IEnumerable<ISelectable> targets)
            //{
            //    this.Town.Map.Events.Post(new PlayerDesignationEvent(null, targets, false));
            //}
            //void add(IEnumerable<ISelectable> targets, DesignationDef des)
            //{
            //    this.Town.Map.Events.Post(new PlayerDesignationEvent(des, targets, false));
            //}
        }
        //internal override void UpdateOrderButtons()
        //{
        //    if (this.Town.Net is Server)
        //        return;
        //    var selectedTargets = SelectionManager.Selected;
        //    var fromblockentities = selectedTargets.Select(i => this.Map.GetBlockEntity(i.Global)).OfType<BlockEntity>().Select(b => b.OriginGlobal.At(this.Town.Map));// new TargetArgs(b.OriginGlobal));
        //    selectedTargets = selectedTargets.Concat(fromblockentities).Distinct();

        //    var areTask = selectedTargets.Where(e => this.Designations.Values.Any(t => t.Contains(e)));// new TargetArgs(e))));
        //    foreach (var d in this.Designations) // need to handle construction designations differently because of multi-celled designations 
        //    {
        //        if (!d.Key.IsManual)
        //            continue;
        //        var selectedDesignations = d.Value.Intersect(selectedTargets);
        //        if (selectedDesignations.Any())
        //            SelectionManager.AddOrderButton(d.Key.IconRemove, remove, selectedDesignations);
        //        else
        //            SelectionManager.RemoveOrderButton(d.Key.IconRemove);
        //    }

        //    var areNotTask = selectedTargets
        //        .Except(areTask)
        //        .Where(t => AllDesignationDefs.Any(d => d.IsValid(t))).ToList();

        //    var splits = AllDesignationDefs.ToDictionary(d => d, d => areNotTask.FindAll(t => d.IsValid(t)));
        //    foreach (var s in AllDesignationDefs)
        //    {
        //        if (!s.IsManual)
        //            continue;
        //        if (!splits.TryGetValue(s, out var list) || list.Count == 0)
        //            SelectionManager.RemoveOrderButton(s.IconAdd);
        //        else
        //            SelectionManager.AddOrderButton(s.IconAdd, targets => add(targets, s), list);
        //    }

        //    void remove(IEnumerable<TargetArgs> targets)
        //    {
        //        this.Town.Map.Events.Post(new PlayerDesignationEvent(null, targets, false));
        //    }
        //    void add(IEnumerable<TargetArgs> targets, DesignationDef des)
        //    {
        //        this.Town.Map.Events.Post(new PlayerDesignationEvent(des, targets, false));
        //    }
        //}
        GroupBox UpdatePendingDesignationLabel(DesignationDef des)
        {
            this.PendingDesignationLabel.ClearControls();
            this.PendingDesignationLabel.AddControlsLineWrap(Label.ParseNewNew("Designation: ", des));
            return this.PendingDesignationLabel;
        }
        internal override void OnTargetSelected(IUISelection info, TargetArgs targetArgs)
        {
            if (this.Designations.FirstOrDefault(d => d.Value.Contains(targetArgs)).Key is DesignationDef des)
                info.AddInfo(this.UpdatePendingDesignationLabel(des));
        }
        public override void DrawUI(SpriteBatch sb, MapBase map, Camera cam)
        {
            foreach(var entityDes in this.EntityDesignations)
            {
                if (!entityDes.Key.IsManual)
                    continue;
                foreach (var entity in entityDes.Value)
                {
                    var icon = entityDes.Key.IconAdd.Icon;
                    icon.DrawFloating(sb, cam, entity);
                }
            }
        }
        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            foreach (var r in this.Renderers)
            {
                if (!r.Key.IsManual)
                    continue;
                r.Value.DrawBlocks(map, cam);
            }
        }

        
    }
}