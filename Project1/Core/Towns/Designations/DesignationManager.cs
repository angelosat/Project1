using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Input.CellRendering;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Digging;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
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
        ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>> Designations;
        ReadOnlyDictionary<DesignationDef, ObservableHashSet<IntVec3>> CellDesignations;
        ReadOnlyDictionary<DesignationDef, ObservableHashSet<Entity>> EntityDesignations;
        ReadOnlyDictionary<DesignationDef, ObservableHashSet<BlockEntity>> BlockEntityDesignations;
        public readonly Dictionary<DesignationDef, BlockRendererObservable> Renderers = [];
        static List<DesignationDef> designationDefs;
        static List<DesignationDef> AllDesignationDefs => designationDefs ??= [.. Def.Get<DesignationDef>()];
        private static readonly IHotkey Hotkey;
        GroupBox _pendingDesignationLabel;
        GroupBox PendingDesignationLabel => this._pendingDesignationLabel ??= new GroupBox();
        static DesignationManager()
        {
            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyCategoryManagement, "Designations", ToggleGui, System.Windows.Forms.Keys.U);

            foreach (var d in Def.Get<DesignationDef>())
                HotkeyManager.RegisterHotkey(ToolManagement.HotkeyCategoryManagement, $"Designate: {d.LabelReadable}", () => SetTool(d));
        }
        
        public DesignationManager(Town town) : base(town)
        {
            var desDefs = Def.Get<DesignationDef>();

            var cellDesignationDefs = desDefs.Where(d => d.TargetType == TargetType.Cell);
            var entityDesignationDefs = desDefs.Where(d => d.TargetType == TargetType.Entity);
            var blockEntityDesignationDefs = desDefs.Where(d => d.TargetType == TargetType.BlockEntity);
            this.CellDesignations = new(cellDesignationDefs.ToDictionary(d => d, d => new ObservableHashSet<IntVec3>()));
            this.EntityDesignations = new(entityDesignationDefs.ToDictionary(d => d, d => new ObservableHashSet<Entity>()));
            this.BlockEntityDesignations = new(blockEntityDesignationDefs.ToDictionary(d => d, d => new ObservableHashSet<BlockEntity>()));

            //this.Designations = new ReadOnlyDictionary<DesignationDef, ObservableHashSet<TargetArgs>>(desDefs.ToDictionary(d => d, d => new ObservableHashSet<TargetArgs>()));

            foreach (var d in desDefs)
            {
                if (d.TargetType == TargetType.Cell)
                    this.Renderers.Add(d, new(this.CellDesignations[d]));
            }

            //foreach (var r in this.Designations.Values)
            //    r.CollectionChanged += this.R_CollectionChanged;

            this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(this.OnBlocksChanged);
            this.Town.Map.Events.ListenTo<EntityDespawnedEvent>(this.OnEntityDespawn);
        }
        //private void R_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (Network.CurrentEndpoint != Ingame.Net)
        //        return;

        //    var removed = e.OldItems?.Cast<TargetArgs>() ?? [];
        //    foreach (var target in removed)
        //    {
        //        if (target.Type == TargetType.Cell)
        //        {
        //            var pos = target.Global;
        //            if (SelectionManager.SingleSelectedCell == pos)
        //                SelectionManager.RemoveInfo(this.PendingDesignationLabel);
        //        }
        //    }

        //    var added = e.NewItems?.Cast<TargetArgs>() ?? [];
        //    foreach (var target in added)
        //        if(target.Type == TargetType.Cell)
        //        {
        //            var pos = target.Global;
        //            if (SelectionManager.SingleSelectedCell == pos)
        //                SelectionManager.AddInfoNew(this.UpdatePendingDesignationLabel(this.Designations.First(d => d.Value.Contains(target)).Key));
        //        }
        //}
        internal IEnumerable<TargetArgs> GetDesignationTargets(DesignationDef desDef)
        {
            return desDef.TargetType switch
            {
                TargetType.Cell => this.CellDesignations[desDef].Select(d => new TargetArgs(this.Map, d)),
                TargetType.Entity => this.EntityDesignations[desDef].Select(d => new TargetArgs(d)),
                TargetType.BlockEntity => this.BlockEntityDesignations[desDef].Select(d => new TargetArgs(d)),
                _ => throw new UnreachableException()
            };
        }
        //internal ObservableHashSet<TargetArgs> GetDesignations(DesignationDef des)
        //{
        //    return this.Designations[des];
        //}
        //internal bool RemoveDesignation(DesignationDef des, TargetArgs target)
        //{
        //    var removed = this.Designations[des].Remove(target);
        //    if (removed)
        //        this.UpdateOrderButtons();
        //    return removed;
        //}
        //internal bool RemoveDesignation(DesignationDef des, IntVec3 target)
        //{
        //    return this.RemoveDesignation(des, target.At(this.Map));
        //}
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
        internal void RemoveInternal(IEnumerable<IntVec3> cells)
        {
            if (!cells.Any())
                return;
            foreach (var cell in cells)
                foreach (var des in this.CellDesignations)
                    des.Value.Remove(cell);
            this.Map.Events.Post(new DesignationsChangedEvent(cells.Select(c => new CellSelection(this.Map, c) as ISelectable)));
        }
        internal void RemoveCells(IEnumerable<IntVec3> cells)
        {
            this.RemoveCells(cells.Select(c => new CellSelection(this.Map, c) as ISelectable));
        }
        internal void RemoveCells(IEnumerable<ISelectable> targets)
        {
            if (!targets.Any())
                return;
            foreach (var cell in targets.OfType<CellSelection>())
                foreach (var des in this.CellDesignations.Where(vk => vk.Key.IsManual))
                    des.Value.Remove(cell.Global);
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        //internal void AddBlockEntities(DesignationDef designation, IEnumerable<BlockEntity> targets, bool isRemoval)
        //{
        //    var removing = isRemoval && designation.IsManual;
        //    var list = this.BlockEntityDesignations[designation];
        //    if (removing)
        //        foreach (var be in targets)
        //            list.Remove(be);
        //    else
        //    {
        //        foreach (var be in targets)
        //            if (designation.Worker.IsValid(be))
        //                list.Add(be);
        //    }
        //    this.Map.Events.Post(new DesignationsChangedEvent(targets));
        //}
        internal void AddCells(DesignationDef designation, IEnumerable<IntVec3> cells, bool isRemoval)
        {
            this.AddCells(designation, cells.Select(c => new CellSelection(this.Map, c) as ISelectable), isRemoval);
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
            this.Map.Events.Post(new DesignationsChangedEvent(targets));
        }
        //internal void Add(DesignationDef designation, IEnumerable<TargetArgs> positions, bool isRemoval)
        //{
        //    if (designation is null)
        //    {
        //        foreach (var l in this.Designations.Where(d => d.Key.IsManual))
        //            foreach (var p in positions)
        //                l.Value.Remove(p);
        //    }
        //    else
        //    {
        //        var list = this.Designations[designation];
        //        foreach (var pos in positions)
        //        {
        //            if (isRemoval && designation.IsManual)
        //                list.Remove(pos);
        //            else if (designation.IsValid(pos) || (pos.Type == TargetType.Cell && this.Map.IsUndiscovered(pos.Global)))
        //                list.Add(pos);
        //        }
        //    }
         
        //    this.UpdateOrderButtons();
        //}
        internal bool IsDesignation(ISelectable target)
        {
            return target switch
            {
                CellSelection => this.CellDesignations.Values.Any(v => v.Contains(target.Global)),
                Entity => this.EntityDesignations.Values.Any(v => v.Contains(target)),
                ////BlockEntity => this.BlockEntityDesignations.Values.Any(v => v.Contains(target)),
                BlockEntity be => this.CellDesignations.Values.Any(v => v.Contains(be.OriginGlobal)),
                _ => false
            };
        }
        internal DesignationDef GetDesignation(ISelectable target)
        {
            return target switch
            {
                CellSelection => this.CellDesignations.FirstOrDefault(v => v.Value.Contains(target.Global)).Key,
                Entity => this.EntityDesignations.FirstOrDefault(v => v.Value.Contains(target)).Key,
                //BlockEntity => this.BlockEntityDesignations.FirstOrDefault(v => v.Value.Contains(target)).Key,
                BlockEntity be => this.CellDesignations.FirstOrDefault(v => v.Value.Contains(be.OriginGlobal)).Key,
                _ => null
            };
        }
        internal bool IsDesignation(IntVec3 global, DesignationDef desType)
        {
            //var contains = this.Designations[desType].Contains(global.At(this.Map));
            var contains = this.CellDesignations[desType].Contains(global);
            return contains;
        }
        internal bool IsDesignation(TargetArgs global, DesignationDef desType)
        {
            return global.Type switch
            {
                TargetType.Cell => this.CellDesignations[desType].Contains(global.Global),
                TargetType.Entity => this.EntityDesignations[desType].Contains(global.Entity),
                //TargetType.BlockEntity => this.BlockEntityDesignations[desType].Contains(global.BlockEntity),
                _ => throw new UnreachableException()
            };
        }
        void OnBlocksChanged(CellsInvalidatedEvent e)
        {
            foreach (var des in this.CellDesignations)
            {
                foreach (var cell in e.Positions)
                {
                    if (!des.Value.Contains(cell))
                        continue;
                    if (!des.Key.Worker.IsValid(new CellSelection(this.Map, cell)))
                        des.Value.Remove(cell);
                }
            }
        }
        void OnEntityDespawn(EntityDespawnedEvent obj)
        {
            foreach (var designations in this.EntityDesignations.Values)
                    designations.Remove(obj.Entity);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            var cellsTag = new SaveTag(SaveTag.Types.Compound, "Cells");
            foreach(var des in this.CellDesignations)
                cellsTag.Add(des.Value.ToList().Save(des.Key.Name));
            var entitiesTag = new SaveTag(SaveTag.Types.Compound, "Entities");
            foreach (var des in this.EntityDesignations)
                entitiesTag.Add(des.Value.Select(e => e.RefId).ToList().Save(des.Key.Name));
            tag.Add(cellsTag);
            tag.Add(entitiesTag);
        }
        public override void Load(SaveTag tag)
        {
            if (tag.TryGetTag("Cells", out var cellsTag))
            {
                foreach (var des in this.CellDesignations)
                {
                    //var array = cellsTag.LoadArrayIntVec3(des.Key.Name);
                    //foreach (var i in array)
                    //    this.CellDesignations[des.Key].Add(i);
                    if(cellsTag.TryLoadArrayIntVec3(des.Key.Name, out var array))
                        foreach (var i in array)
                            this.CellDesignations[des.Key].Add(i);
                }
            }
            if (tag.TryGetTag("Entities", out var entitiesTag))
            {
                foreach (var des in this.EntityDesignations)
                {
                    var entities = this.Map.World.GetEntities(entitiesTag.LoadListInt(des.Key.Name));
                    foreach (var i in entities)
                        this.EntityDesignations[des.Key].Add(i);
                }
            }

            RefreshConstructionDesignations();
        }

        private void RefreshConstructionDesignations()
        {
            return;
            foreach (var be in this.Map.BlockEntities)
                if (be.HasComp<BlockConstructionComp>())
                    foreach (var cell in be.CellsOccupied)
                        this.CellDesignations[DesignationDefOf.Construct].Add(cell);
        }

        public override void Write(IDataWriter w)
        {
            foreach (var d in this.CellDesignations)
            {
                w.Write(d.Value.Count);
                foreach (var cell in d.Value)
                    w.Write(cell);
            }
            foreach (var d in this.EntityDesignations)
            {
                w.Write(d.Value.Count);
                foreach (var entity in d.Value)
                    w.Write(entity.RefId);
            }
        }
        public override void Read(IDataReader r)
        {
            foreach (var d in this.CellDesignations)
            {
                var count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    d.Value.Add(r.ReadIntVec3());
            }
            foreach (var d in this.EntityDesignations)
            {
                var count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    d.Value.Add(this.Map.World.GetEntity(r.ReadEntityRefId()));
            }
        }
        internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
        {
            yield return (() => $"Designations [{Hotkey.GetLabel()}]", ToggleGui);
        }
        private static readonly Lazy<Control> _guiNew = new(() => ContextMenuManager.CreateContextSubMenu("Designations", GetContextSubmenuItems()).HideOnAnyClick());
        static void ToggleGui()
        {
            _guiNew.Value.Toggle();
        }
        static IEnumerable<(string, Action)> GetContextSubmenuItems()
        {
            yield return ("Remove", () => SetTool(null));
            foreach (var def in AllDesignationDefs.Where(d => d.IsManual))
                yield return (def.LabelReadable, () => SetTool(def));
        }
        private static void SetTool(DesignationDef d)
        {
            ToolManager.SetTool(new ToolDesignation((begin, end, isRemoval) => Ingame.Instance.Events.Post(new PlayerDesignationCellsEvent(d, begin, end, isRemoval))));
        }
        GroupBox UpdatePendingDesignationLabel(DesignationDef des)
        {
            this.PendingDesignationLabel.ClearControls();
            this.PendingDesignationLabel.AddControlsLineWrap(Label.ParseNewNew("Designation: ", des));
            return this.PendingDesignationLabel;
        }
        //internal override void OnTargetSelected(IUISelection info, TargetArgs targetArgs)
        //{
        //    if (this.Designations.FirstOrDefault(d => d.Value.Contains(targetArgs)).Key is DesignationDef des)
        //        info.AddInfo(this.UpdatePendingDesignationLabel(des));
        //}
        public override void DrawUI(SpriteBatch sb, MapBase map, Camera cam)
        {
            foreach(var entityDes in this.EntityDesignations)
            {
                if (!entityDes.Key.IsManual)
                    continue;
                foreach (var entity in entityDes.Value)
                {
                    //var icon = entityDes.Key.IconAdd.Icon;
                    entityDes.Key.Icon?.DrawFloating(sb, cam, entity);
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