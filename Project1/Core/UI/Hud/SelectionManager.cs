using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Input.CellRendering;
using Project1.Core.Input.Orders;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Zones;
using Project1.Framework;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.UI.Hud
{
    [EnsureStaticCtorCall]
    public sealed class SelectionManager
    {
        readonly GroupBox BoxTabs, BoxOrderButtons, BoxIcons, BoxInfo;
        public Panel PanelInfo;
        public Label LabelName;
        readonly IconButton IconInfo, IconCenter, IconDetails;
        readonly IconButton IconCycle;
        readonly IconButton IconIssues;
        readonly BlockRendererNew Renderer = new(Block.BlockHighlight);
        //static List<OrderCommandDef> _allOrderCommands;
        static readonly Lazy<List<OrderCommandDef>> AllOrderCommands = new(() => [.. Def.GetDefs<OrderCommandDef>()]);
        static SelectionManager()
        {
        }
        static readonly IconButton IconSlice = new(Icon.ArrowDown)
        {
            BackgroundTexture = UIManager.Icon16Background,
            LeftClickAction = ToolManagement.Slice,
            HoverText = "Slice z-level"
        };
        public static readonly SelectionManager Instance = new();
        public void Bind(NetEndpoint net)
        {
            var map = net.Map;
            map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
            map.Events.ListenTo<BlockEntityRemovedEvent>(OnBlockEntityRemoved);
            map.Events.ListenTo<BlockEntityUpdatedEvent>(OnBlockEntityUpdated);

            map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);
            map.Events.ListenTo<CellsInvalidatedEvent>(OnBlocksUpdated);
        }

        internal void Init(Ingame ingame)
        {
            ingame.Events.ListenTo<PlayerSelectionEvent>(OnPlayerSelection);
            ingame.Events.ListenTo<PlayerSelectionCubeEvent>(OnPlayerSelectionCube);
            ingame.Events.ListenTo<PlayerSelectionRectangleEvent>(OnPlayerSelectionRectangle);
        }

        private void OnPlayerSelectionRectangle(PlayerSelectionRectangleEvent e)
        {
            this.SelectInternal(e.Entities);
        }

        private void OnPlayerSelectionCube(PlayerSelectionCubeEvent e)
        {
            var begin = e.Begin;
            var end = e.End;
            if(begin == end)
                this.SelectSingle(new CellSelection(Ingame.GetMap(), begin));
            else
                this.Select(begin, end);
        }


        private void OnPlayerSelection(PlayerSelectionEvent e)
        {
            Select(e.Single);
        }

        private void OnBlocksUpdated(CellsInvalidatedEvent e)
        {
            //if (this.Selectable is TargetArgs target && target.Type == TargetType.Cell && e.Positions.Contains(target.Global))
            if (this.Selectable is CellSelection cell && e.Positions.Contains(cell.Global))
                this.Unselect();
        }

        private void OnEntityDespawned(EntityDespawnedEvent e)
        {
            if (this.Selectable == e.Entity)
                this.Unselect();
            else
            {
                //if (this.MultipleSelected.FirstOrDefault(t => t.Object == e.Entity) is TargetArgs t)
                if (this.MultipleSelected.FirstOrDefault(t => t == e.Entity) is ISelectable t)
                        this.MultipleSelected.Remove(t);
                if (this.MultipleSelected.Count == 0)
                {
                    this.PanelInfo.Hide();
                    this.BoxTabs.Hide();
                }
            }
        }
        void Unselect(ISelectable t)
        {
            this.MultipleSelected.Remove(t);
            this.Renderer.Invalidate();
            this.BoxOrderButtons.ClearControls();
            var map = Ingame.GetMap();
            foreach (var target in this.MultipleSelected)
                map.Town.Select(target, this);
        }
        void Unselect(TargetArgs t)
        {
            this.MultipleSelected.Remove(t);
            this.Renderer.Invalidate();
            this.BoxOrderButtons.ClearControls();
            //foreach (var target in this.MultipleSelected)
            //    target.Map.Town.Select(target, this);
        }
        private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (this.Selectable == e.Entity)
                this.Unselect();
            else
            {
                if (this.MultipleSelected.FirstOrDefault(t =>
                     //t.BlockEntity == e.Entity ||
                     //t.Global == (Vector3)e.Entity.OriginGlobal) is TargetArgs t)
                     t == e.Entity ||
                     t is TargetArgs target && target.Global == (Vector3)e.Entity.OriginGlobal) is ISelectable t)
                {
                    this.Unselect(t);
                }
                if (this.MultipleSelected.Count == 0)
                {
                    this.PanelInfo.Hide();
                    this.BoxTabs.Hide();
                    this.BoxIcons.Hide();
                    this.BoxOrderButtons.Hide();
                }
            }
        }
        private void OnBlockEntityUpdated(BlockEntityUpdatedEvent e)
        {
            if (this.Selectable == e.Entity)
                this.PanelInfo.Invalidate(true);
        }
        private void OnZoneDeleted(ZoneDeletedEvent e)
        {
            if (this.Selectable == e.Zone)
                this.Unselect();
        }
        void Unselect()
        {
            //this.SelectSingle(TargetArgs.Null);
            this.SelectSingle(null);
        }
        public ISelectable SelectedSource;// = TargetArgs.Null;
        ISelectable Selectable;
        Window WindowInfo;
        //IEnumerator<ISelectable> SelectedStack;
        IReadOnlyList<ISelectable> SelectedStackNew;
        ISelectable SelectedStackCurrent => this.SelectedStackNew?[this._selectedStackIndex];
        int _selectedStackIndex;
        HashSet<ISelectable> MultipleSelected => this.Selection.Targets;
        readonly SelectionFinal Selection = new();
        public IReadOnlyCollection<ISelectable> CurrentSelections => this.Selection.Targets;
        SelectionIntent CurrentSelection;

        SelectionManager()
        {
            this.PanelInfo = Panel.FromClientSize(302, Label.DefaultHeight * 6);// 100); (302 = fit 3 x 100px widt bars, width 1 px spacing between them
            this.BoxTabs = new GroupBox()
            {
                AutoSize = false,
                Size = new Rectangle(0, 0, this.PanelInfo.Width, Button.DefaultHeight)
            };
            this.BoxTabs.AnchorTo(() => this.PanelInfo.ScreenLocation, Vector2.UnitY);

            this.PanelInfo.AnchorToBottomCenter();
            this.LabelName = new Label() { TextFunc = () => "<none>" };
            Lazy<SelectionDetailsGui> detailsGui = new Lazy<SelectionDetailsGui>(() => new SelectionDetailsGui());
            this.IconDetails = new IconButton("^")
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = () =>
                {
                    detailsGui.Value.Refresh(Instance.SelectedSource ?? Instance.SelectedStackCurrent).GetOrCreateWindow("Details").Toggle();
                },
                HoverText = "Details"
            };
            this.IconInfo = new IconButton("?")
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = ToggleInfo,
                HoverText = "Inspect"
            };
            this.IconCenter = new IconButton(Icon.ArrowUp)
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = CenterCamera,
                HoverText = "Center camera"
            };
            this.IconCycle = new IconButton(Icon.Replace)
            {
                BackgroundTexture = UIManager.Icon16Background,
                //LeftClickAction = this.CycleTargets,
                LeftClickAction = this.CycleTargetsNew,
                HoverText = "Cycle targets"
            };

            this.IconIssues = new IconButton("!") { BackgroundTexture = UIManager.Icon16Background, TooltipFunc = showIssuesTooltip }
                .Flash(true)
                .VisibleWhen(() => SelectedBlockEntity?.Errors.Any() ?? false) as IconButton;

            static void showIssuesTooltip(Control tooltip)
            {
                if (SelectedBlockEntity is BlockEntity blentity)
                    tooltip.AddControlsBottomLeft(blentity.GetErrorsGui());
            }

            this.BoxIcons = new GroupBox();
            this.PopulateBoxIcons();

            this.BoxOrderButtons = new GroupBox();
            this.BoxOrderButtons.BackgroundColorFunc = () => Color.Black * .5f;
            this.BoxOrderButtons.LocationFunc = () => this.PanelInfo.BottomRight;
            this.BoxOrderButtons.Anchor = new Vector2(0, 1);
            this.BoxOrderButtons.ControlsChangedAction = () => this.BoxOrderButtons.AlignLeftToRight();

            this.BoxInfo = new GroupBox() { Location = this.LabelName.BottomLeft };
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxIcons,
                this.BoxInfo
                );

        }

        private void RepositionsBoxIcons()
        {
            this.BoxIcons.AlignLeftToRight();
            this.BoxIcons.Location = new Vector2(this.PanelInfo.ClientSize.Right, this.PanelInfo.ClientSize.Top);
            this.BoxIcons.Anchor = new Vector2(1, 0);
        }

        private void PopulateBoxIcons()
        {
            this.BoxIcons.ClearControls();
            this.BoxIcons.AddControls(
                IconIssues,
                IconSlice,
                this.IconCenter,
                this.IconInfo,
                this.IconDetails
                );

            if (this.SelectedStackNew is not null)
                this.BoxIcons.AddControls(this.IconCycle);

            this.RepositionsBoxIcons();
        }

        private void CenterCamera()
        {
            if (this.SelectedSource is not null)
                //if (this.SelectedSource.Type != TargetType.Null)
                    ScreenManager.CurrentScreen.Camera.CenterOn(this.SelectedSource.Global);
        }
        public void SetName(string text)
        {
            this.LabelName.TextFunc = () => text;
        }
        private static void ToggleInfo()
        {
            if (Instance.SelectedSource is Inspectable obj)
                Inspector.Refresh(obj);
            else
            //{
                if (Instance.SelectedStackCurrent is Inspectable insp)
                    Inspector.Refresh(insp);
            //    else
            //        Inspector.Refresh(Instance.SelectedSource);
            //}
            Inspector.Show();
        }

        public static void Select(TargetArgs target)
        {
            ISelectable selectable = target.Type switch
            {
                TargetType.Entity => target.Entity,
                TargetType.BlockEntity => target.BlockEntity,
                TargetType.Cell => new CellSelection(target.Map, target.Global, target.Face),
                _ => throw new UnreachableException()
            };
            Instance.SelectSingle(selectable);
        }
        public static void Select(MapBase map, BoundingBox box)
        {
            Select(map.GetObjects(box).Select(s => new TargetArgs(s)));
        }
        public static void Select(MapBase map, IntVec3 begin, IntVec3 end)
        {
            var box = new BoundingBox(begin, end).ToListIntVec3();
            var cells = box.Select(c => new TargetArgs(map, c)).Where(t => t.Cell.Block != BlockDefOf.Air.Worker || t.BlockEntityOld is not null);
            Select(cells);
        }

        private void Select(IntVec3 begin, IntVec3 end)
        {
            this.Selection.SetBox(Ingame.CurrentMap, begin, end);
            this.Renderer.Invalidate();
            this.LabelName.TextFunc = () => $"Multiple cells x{this.Selection.Cells.Count}";
            this.RefreshOrderButtons();
        }
        public static void Select(IEnumerable<GameObject> entities)
        {
            Select(entities.Select(e => new TargetArgs(e)));
        }
        public static void Select(IEnumerable<TargetArgs> targets)
        {
            Instance.SelectInternal(targets);
        }
        public static void Select(IEnumerable<ISelectable> targets)
        {
            Instance.SelectInternal(targets);
        }
        /// <summary>
        /// why did i have this commented out?
        /// because it doesn't set the map field in targetargs
        /// </summary>
        /// <param name="cells"></param>
        public static void Select(MapBase map, IEnumerable<IntVec3> cells)
        {
            Instance.SelectInternal(cells.Select(c => c.At(map)));
        }

        internal void OnCameraRotated(Camera camera)
        {
            this.Renderer.Invalidate();
        }

        internal static void SelectAllVisible(ItemDef def)
        {
            var objects = Ingame.Instance.Scene.ObjectsDrawn.Where(i => i.Def == def).Select(o => new TargetArgs(o));
            Select(objects);
        }
        internal static void AddToSelection(IEnumerable<GameObject> targets)
        {
            AddToSelection(targets.Select(o => new TargetArgs(o)));
        }
        //internal static void AddToSelection(IEnumerable<TargetArgs> targets)
        //{
        //    var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2.IsEqual(t))).Concat(targets).ToList();
        //    Instance.SelectInternal(list);
        //}
        internal static void AddToSelection(IEnumerable<ISelectable> targets)
        {
            var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2 == t)).Concat(targets).ToList();
            Instance.SelectInternal(list);
        }
        internal static void AddToSelection(ISelectable target)
        {
            //var existing = Instance.MultipleSelected.FirstOrDefault(t => t.IsEqual(target));
            var existing = Instance.MultipleSelected.FirstOrDefault(t => t == target);
            if (existing is not null)
                Instance.SelectInternal(Instance.MultipleSelected.Except([existing]));
            else
                Instance.SelectInternal([.. Instance.MultipleSelected, target]);
        }
        //internal static void AddToSelection(TargetArgs target)
        //{
        //    var existing = Instance.MultipleSelected.FirstOrDefault(t => t.IsEqual(target));
        //    if (existing != null)
        //        Instance.SelectInternal(Instance.MultipleSelected.Except([existing]));
        //    else
        //        Instance.SelectInternal([.. Instance.MultipleSelected, target]);
        //}
        private static IEnumerable<ISelectable> FilterActors(IEnumerable<ISelectable> targets)
        {
            //if (targets.Any(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>()))
            //    return targets.Where(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>());
            return targets.OfType<Actor>().Where(i => i.HasComponent<NpcComponent>());
        }

        private void SelectInternal(IEnumerable<ISelectable> targets)
        {
            this.SelectSingle(null);
            var selectedActors = FilterActors(targets).Where(t => t.Exists);
            var targetsFinal = (selectedActors.Any() ? selectedActors : targets).ToList();
            if (targetsFinal.Count == 0)
                return;
            if (targetsFinal.Count == 1)
            {
                this.SelectSingle(targetsFinal.First());
                return;
            }
            this.Selection.Targets = [.. targetsFinal];
            this.LabelName.TextFunc = () => $"Multiple x{this.MultipleSelected.Count}";

            this.CreateButtons(targets);
            this.PanelInfo.RemoveControls(this.BoxIcons);
            this.Show();

            //this.SelectSingle(null);
            //var selectedActors = FilterActors(targets).Where(t => t.Exists);
            //this.Selection.Targets = [.. selectedActors.Any() ? selectedActors : targets];
            //if (this.MultipleSelected.Count == 0)
            //    return;
            //if (this.MultipleSelected.Count == 1)
            //{
            //    this.SelectSingle(targets.First());
            //    return;
            //}

            //this.LabelName.TextFunc = () => $"Multiple x{this.MultipleSelected.Count}";

            //this.CreateButtons(targets);
            //this.PanelInfo.RemoveControls(this.BoxIcons);
            //this.Show();
        }
        private void SelectSingle(ISelectable target)
        {
            this.Renderer.Invalidate();

            if (this.SelectedSource == target)
            {
                this.CycleTargetsNew();
                return;
            }
            this.Selection.Clear();
            this.SelectedSource = target;
            //this.SelectedStack = null;
            this.SelectedStackNew = null;
            this.WindowInfo = null;
            this.Clear();
            switch (target)
            {
                case Entity entity:// TargetType.Entity:
                    //var entity = target as Entity;//.Object;
                    this.LabelName.TextFunc = () => entity.Name;
                    //this.MultipleSelected.Clear();
                    //this.MultipleSelected.Add(target);
                    this.Selection.Add(target);
                    entity.GetSelectionInfo(this);
                    entity.GetQuickButtons(this);
                    this.InitInfoTabs(entity.GetQuickButtons(), target);
                    entity.Map?.Town?.Select(target, this);
                    this.InitInfoTabs(entity.Map?.Town?.GetTabs(target));
                    break;

                case CellSelection cell:// TargetType.Cell:
                    //this.MultipleSelected.Clear();
                    //this.MultipleSelected.Add(target);
                    this.Selection.Add(target);
                    //var selectables = cell.Map.Town.QuerySelectables(cell);
                    //this.SelectedStack = selectables.GetEnumerator();
                    this.SelectedStackNew = cell.Map.Town.QuerySelectablesNew(cell);
                    this._selectedStackIndex = 0;
                    //this.CycleTargets();
                    this.CycleTargetsNew();
                    if (cell.Map.IsUndiscovered(cell.Global))
                        this.LabelName.TextFunc = () => "Unknown block";
                    break;

                case BlockEntity blockEntity:// TargetType.BlockEntity:
                    //this.MultipleSelected.Clear();
                    //this.MultipleSelected.Add(target);
                    this.Selection.Add(target);

                    this.SetName(target.Name);

                    target.GetSelectionInfo(this);
                    target.GetQuickButtons(this);
                    this.InitInfoTabs(target.GetInfoTabs());
                    blockEntity.Map.Town.Select(target, this);
                    break;

                case null:// TargetType.Null:
                    this.PanelInfo.Hide();
                    this.BoxOrderButtons.Hide();
                    this.LabelName.TextFunc = () => "<none>";
                    if (this.WindowInfo != null)
                        this.WindowInfo.Hide();
                    this.WindowInfo = null;
                    this.SelectedSource = TargetArgs.Null;
                    this.Selectable = null;
                    this.MultipleSelected.Clear();
                    return;

                default:
                    throw new ArgumentException($"{nameof(target)} is not an {nameof(ISelectable)}");
            }
            this.SelectedSource = target;
            this.Show();

            this.PanelInfo.WindowManager.OnSelectedTargetChanged(target);
            this.PanelInfo.Validate(true);

            this.RefreshOrderButtons();
        }
        void Show()
        {
            this.BoxTabs.Show();
            this.PanelInfo.Show();
            this.BoxOrderButtons.Show();
        }

        void Hide()
        {
            this.BoxTabs.Hide();
            this.PanelInfo.Hide();
            this.BoxOrderButtons.Hide();
        }
        private void Clear()
        {
            foreach (var a in this.ActionsAdded)
                a.Value.Clear();
            this.ActionsAdded.Clear();
            this.BoxTabs.ClearControls();
            this.BoxOrderButtons.ClearControls();
            this.BoxInfo.ClearControls();
            this.PanelInfo.ClearControls();
            this.PopulateBoxIcons();
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxInfo,
                this.BoxIcons);
        }

        //private void CycleTargets()
        //{
        //    if (this.SelectedStack == null)
        //        return;
        //    this.SelectedStack.MoveNext();
        //    var first = this.SelectedStack.Current;
        //    this.SetName(first.Name);
        //    this.Clear();

        //    first.GetSelectionInfo(this);
        //    first.GetQuickButtons(this);
        //    this.InitInfoTabs(first.GetInfoTabs());
        //    Client.Instance.Map.Town.Select(first, this);
        //    this.Selectable = first;
        //}
        private void CycleTargetsNew()
        {
            if (this.SelectedStackNew is null)
                return;
            this._selectedStackIndex = (this._selectedStackIndex + 1) % this.SelectedStackNew.Count;
            var current = this.SelectedStackNew[this._selectedStackIndex];
            this.SetName(current.Name);
            this.Clear();

            current.GetSelectionInfo(this);
            current.GetQuickButtons(this);
            this.InitInfoTabs(current.GetInfoTabs());
            Client.Instance.Map.Town.Select(current, this);
            this.Selectable = current;
        }
        void InitInfoTabs(IEnumerable<(string, Type)> tabs, ISelectable selectable)
        {
            foreach (var (label, guiType) in tabs)
                this.AddTabAction(label, () => UIManager.ToggleSingleton(guiType, selectable), Color.Orange);
            //this.AddTabAction(label, () => UIManager.ToggleUnique(guiType, selectable), Color.Orange);
        }
        void InitInfoTabs(IEnumerable<(string name, Action action)> tabs)
        {
            foreach (var (name, action) in tabs)
                this.AddTabAction(name, action, Color.Orange);
        }
        void InitInfoTabs(IEnumerable<Button> tabs)
        {
            if (tabs is null)
                return;
            foreach (var button in tabs)
                this.AddTabAction(button);
        }
        

        readonly Dictionary<Action<List<ISelectable>>, List<ISelectable>> ActionsAdded = [];

        private void CreateButtons(IEnumerable<ISelectable> targets)
        {
            this.BoxOrderButtons.ClearControls();
            this.ActionsAdded.Clear();
            foreach (var tar in targets)
                tar.GetQuickButtons(this);
            Client.Instance.Map.Town.Select(null, this);
            this.RefreshOrderButtons();
        }

        void AddTabAction(Button button)
        {
            button.BackgroundColor = UIManager.TintPrimary * .5f;
            this.BoxTabs.AddControlsLineWrap([button], this.PanelInfo.Width);
        }
        void AddTabAction(string label, Action action, Color col)
        {
            this.BoxTabs.AddControlsLineWrap([new Button(label) { LeftClickAction = action, BackgroundColor = col * .5f }], this.PanelInfo.Width);
        }
        public void AddTabAction(string label, Action action)
        {
            this.AddTabAction(label, action, Color.PaleVioletRed);
        }
        internal void AddTabs(params Button[] buttons)
        {
            this.BoxTabs.AddControls(buttons);
        }

        internal static void AddButton(IconButton button)
        {
            Instance.AddButtons([button]);
        }
        private void MultipleSelectedAction(Action<List<ISelectable>> action)
        {
            action(this.ActionsAdded[action]);
        }

        //public void Update()
        //{
        //    /// move this to ongameevent?
        //    if (this.SelectedSource is not null && this.SelectedSource.Type == TargetType.Entity && this.SelectedSource.Object.IsDisposed)
        //        this.SelectSingle(TargetArgs.Null);

        //    if (this.Selectable is null)
        //    {
        //        if (!this.MultipleSelected.Any())
        //            if (this.PanelInfo.IsOpen)
        //                this.Hide();
        //        return;
        //    }

        //    /// do i really need this? i handle the blockschanged message anyway, and this causes problems for selecting undiscovered air blocks 
        //    if (!this.Selectable.Exists)
        //        this.SelectSingle(TargetArgs.Null);
        //}
        public void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            if(this.Selection.Cells.Count > 0)
            {
                var map = Ingame.GetMap();
                //var first = this.MultipleSelected.First();
                //var map = first.Map;
                this.Renderer.DrawBlocks(map, camera, this.Selection.Cells);
                //else if (first is BlockEntity blockEntity)
                //{
                //    Renderer.DrawBlocks(map, camera, blockEntity.CellsOccupied);

                //    // HACK: using this to make the origin block draw its interaction spot, if any
                //    map.GetBlock(blockEntity.OriginGlobal).DrawSelected(sb, camera, map, blockEntity.OriginGlobal);
                //}
                //if (SingleSelectedCell.HasValue)
                //{
                //    var singleCell = SingleSelectedCell.Value;
                //    map.GetBlock(singleCell).DrawSelected(sb, camera, map, singleCell);
                //}
            }
            return;
            if (this.MultipleSelected.Count != 0)
            {
                var first = this.MultipleSelected.First();
                var map = first.Map;
                //if (first.Type == TargetType.Cell)
                if (first is CellSelection)
                    this.Renderer.DrawBlocks(map, camera, this.MultipleSelected.Select(t => (IntVec3)t.Global));
                //else if (first.Type == TargetType.BlockEntity)
                else if (first is BlockEntity blockEntity)
                {
                    this.Renderer.DrawBlocks(map, camera, blockEntity.CellsOccupied);

                    // HACK: using this to make the origin block draw its interaction spot, if any
                    map.GetBlock(blockEntity.OriginGlobal).DrawSelected(sb, camera, map, blockEntity.OriginGlobal);
                }
                if (SingleSelectedCell.HasValue)
                {
                    var singleCell = SingleSelectedCell.Value;
                    map.GetBlock(singleCell).DrawSelected(sb, camera, map, singleCell);
                }
            }
            else if (this.SelectedSource is not null) // this block never executes aymore because even single selections are contained in the multipleselected collection
            {
                throw new UnreachableException();
                //if (this.SelectedSource.Type == TargetType.Cell)
                //{
                //    camera.DrawCellHighlights(Block.BlockHighlight, [this.SelectedSource.Global], Color.Yellow);
                //    var map = this.SelectedSource.Map;
                //    var global = this.SelectedSource.Global;
                //    map.GetBlock(global).DrawSelected(sb, camera, map, global);
                //}
                //if (this.SelectedSource.Type == TargetType.BlockEntitySlot)
                //{

                //}
            }
        }
        public void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Count == 0)
                return;

            foreach (var obj in this.MultipleSelected)
            {
                if (obj is Entity entity && entity.Map == Ingame.CurrentMap)
                    entity.DrawBorder(sb, camera);
                else if (this.SelectedSource is Entity entitySource)
                    entitySource.DrawBorder(sb, camera);
            }
            //if (this.MultipleSelected.Any())
            //    foreach (var obj in this.MultipleSelected)
            //        if (obj.Type == TargetType.Entity && obj.Object.Map == Ingame.CurrentMap)
            //            obj.Object.DrawBorder(sb, camera);
            //        else if (this.SelectedSource != null)
            //            if (this.SelectedSource.Type == TargetType.Entity)
            //                this.SelectedSource.Object.DrawBorder(sb, camera);
        }

        //public static bool IsSelected(TargetArgs tar)
        //{
        //    return Instance.MultipleSelected.Any(t => t.IsEqual(tar)) || Instance.SelectedSource.IsEqual(tar);
        //}
        //public static bool IsSelected(IntVec3 tar)
        //{
        //    return
        //        Instance.MultipleSelected.Any(t => t.Type == TargetType.Cell && (IntVec3)t.Global == tar) ||
        //        Instance.SelectedSource.Type == TargetType.Cell && (IntVec3)Instance.SelectedSource.Global == tar;
        //}
        internal static bool IsSelected(ISelectable item)
        {
            //if (Instance.SelectedStackNew is null)
            //    return false;
            return Instance.SelectedStackCurrent == item;
        }
        internal static bool ClearTargets()
        {
            //if (Instance.MultipleSelected.Count == 0 && Instance.SelectedSource.Type == TargetType.Null)
            if (Instance.MultipleSelected.Count == 0 && Instance.SelectedSource is null)
                return false;
            Instance.SelectSingle(TargetArgs.Null);
            return true;
        }

        public void AddIcon(IconButton icon)
        {
            if (this.MultipleSelected.Count > 1)
                return;

            this.BoxIcons.Controls.Insert(0, icon);
            this.RepositionsBoxIcons();
        }

        public void AddInfo(Control ctrl)
        {
            this.BoxInfo.AddControls(ctrl);
            this.BoxInfo.Controls.AlignVertically();
        }
        public static void RemoveInfo(Control ctrl)
        {
            Instance.BoxInfo.RemoveControls(ctrl);
        }
        public static void AddInfoNew(Control ctrl)
        {
            Instance.BoxInfo.AddControls(ctrl);
        }
        public static void RemoveOrderButton(IconButton button)
        {
            Instance.BoxOrderButtons.RemoveControls(button);
        }

        internal void AddButtons(params IconButton[] buttons)
        {
            this.BoxOrderButtons.AddControls(buttons);
        }
        //internal void AddButton(IconButton button, Action<List<ISelectable>> action, ISelectable obj, bool singleTargetOnly = false)
        //{
        //    this.AddButton(button, action, new TargetArgs(obj), singleTargetOnly);
        //}
        internal void AddButton(IconButton button, Action<ISelectable> action, ISelectable target)
        {
            this.AddButton(button, targets => action(targets.First()), target, true);
        }
        internal void AddButton(IconButton button, Action<List<ISelectable>> action, ISelectable obj, bool singleTargetOnly = false)
        {
            if (singleTargetOnly && this.MultipleSelected.Count > 1)
                return;

            if (this.ActionsAdded.TryGetValue(action, out List<ISelectable> existing))
            {
                existing.Add(obj);
                return;
            }
            else
                this.ActionsAdded.Add(action, [obj]);
            button.LeftClickAction = () => this.MultipleSelectedAction(action);
            this.BoxOrderButtons.AddControls(button);
        }
        //internal static void AddButton(IconButton button, Action<List<ISelectable>> action, IEnumerable<GameObject> targets)
        //{
        //    AddOrderButton(button, action, targets.Select(t => new TargetArgs(t)));
        //}
        internal static void AddButton(IconButton button, Action<List<ISelectable>> action, IEnumerable<IntVec3> cells)
        {
            AddOrderButton(button, action, cells.Select(t => t.At(Client.Instance.Map)));
        }
        internal static void AddOrderButton(IconButton button, Action<List<ISelectable>> action, IEnumerable<ISelectable> targets)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out var existing))
                Instance.ActionsAdded.Remove(action);
            Instance.ActionsAdded.Add(action, targets.ToList());
            if (!Instance.BoxOrderButtons.Controls.Contains(button))
                Instance.BoxOrderButtons.AddControls(button);
            button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
        }
        internal void Select(SelectionIntent selection)
        {
            this.CurrentSelection = selection;
            //Select(selection.ResolveTargets(Ingame.GetMap()));
            Select(selection.Resolve(Ingame.GetMap()));
        }
        internal void RefreshOrderButtons()
        {
            this.BoxOrderButtons.ClearControls();
            var map = Ingame.GetMap(); // temp
            //var targets = this.CurrentSelection.ResolveTargets(map);
            var targets = this.CurrentSelections;
            foreach (var orderdef in AllOrderCommands.Value)
            {
                if (targets.Any(orderdef.Worker.CanIssue))
                {
                    var runtime = new OrderCommandRuntime(orderdef);
                    var button = new QuickButton(new Icon(orderdef.Sprite), null, orderdef.LabelReadable)
                    {
                        //LeftClickAction = () => runtime.Issue(this.CurrentSelection)
                        LeftClickAction = () => runtime.Issue(this.Selection.ToSelectionIntent())
                    };
                    this.BoxOrderButtons.AddControls(button);
                }
            }
        }
        //void UpdateOrderButtons()
        //{
        //    var selected = this.MultipleSelected;
        //    foreach (var orderdef in AllOrderCommands.Value)
        //    {
        //        if (selected.Any(orderdef.Worker.CanIssue))
        //        {
        //            var runtime = new OrderCommandRuntime(orderdef);
        //            var button = new QuickButton(new Icon(orderdef.Sprite), null, orderdef.LabelReadable)
        //            {
        //                LeftClickAction = () => runtime.Issue(selected)
        //            };
        //            this.BoxOrderButtons.AddControls(button);
        //        }
        //    }
        //}
        internal static void AddButton(IconButton button, Action<List<ISelectable>> action, ISelectable target)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out List<ISelectable> existing))
            {
                existing.Add(target);
            }
            else
            {
                Instance.ActionsAdded.Add(action, [target]);
                Instance.BoxOrderButtons.AddControls(button);
                button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
            }
        }
        //internal static IEnumerable<ISelectable> Selected => Instance.MultipleSelected;
        internal static IEnumerable<Entity> GetSelectedEntities()
        {
            return Instance.CurrentSelections
                .OfType<Entity>();
                //.Where(tar => tar.Type == TargetType.Entity)
                //.Select(t => t.Object);
        }
        internal IEnumerable<CellSelection> GetSelectedCells()
        {
            return this.CurrentSelections.OfType<CellSelection>();
            //return this.MultipleSelected
            //    .Where(tar => tar.Type == TargetType.Cell)
            //    .Select(t => (IntVec3)t.Global);
        }
        internal static IEnumerable<CellSelection> SelectedCells => Instance.GetSelectedCells();
        internal static IEnumerable<GameObject> SelectedEntities => GetSelectedEntities();

        internal static ISelectable SingleSelected => Instance.MultipleSelected.Count == 1 ? Instance.MultipleSelected.Single() : null;
        internal static Entity SingleSelectedEntity => SingleSelected as Entity;
        internal static IntVec3? SingleSelectedCell => (SingleSelected is TargetArgs target && target.Type == TargetType.Cell) ? target.Global : null;
        internal static BlockEntity SelectedBlockEntity => (SingleSelected is TargetArgs target && target.Type == TargetType.Cell) ? target.BlockEntityOld : null;
    }

    //[EnsureStaticCtorCall]
    //public sealed class SelectionManager
    //{
    //    readonly GroupBox BoxTabs, BoxOrderButtons, BoxIcons, BoxInfo;
    //    public Panel PanelInfo;
    //    public Label LabelName;
    //    readonly IconButton IconInfo, IconCenter, IconDetails;
    //    readonly IconButton IconCycle;
    //    readonly IconButton IconIssues;
    //    static readonly BlockRendererNew Renderer = new(Block.BlockHighlight);
    //    //static List<OrderCommandDef> _allOrderCommands;
    //    static readonly Lazy<List<OrderCommandDef>> AllOrderCommands = new(()=>[.. Def.GetDefs<OrderCommandDef>()]);
    //    static SelectionManager()
    //    {
    //    }
    //    static readonly IconButton IconSlice = new(Icon.ArrowDown)
    //    {
    //        BackgroundTexture = UIManager.Icon16Background,
    //        LeftClickAction = ToolManagement.Slice,
    //        HoverText = "Slice z-level"
    //    };
    //    public static readonly SelectionManager Instance = new();
    //    public void Bind(NetEndpoint net)
    //    {
    //        var map = net.Map;
    //        map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
    //        map.Events.ListenTo<BlockEntityRemovedEvent>(OnBlockEntityRemoved);
    //        map.Events.ListenTo<BlockEntityUpdatedEvent>(OnBlockEntityUpdated);

    //        map.Events.ListenTo<ZoneDeletedEvent>(OnZoneDeleted);
    //        map.Events.ListenTo<CellsInvalidatedEvent>(OnBlocksUpdated);
    //    }

    //    internal void Init(Ingame ingame)
    //    {
    //        ingame.Events.ListenTo<PlayerSelectionEvent>(OnPlayerSelection);
    //    }

    //    private void OnPlayerSelection(PlayerSelectionEvent e)
    //    {
    //        Select(e.Single);
    //    }

    //    private void OnBlocksUpdated(CellsInvalidatedEvent e)
    //    {
    //        if (this.Selectable is TargetArgs target && target.Type == TargetType.Position && e.Positions.Contains(target.Global))
    //            this.Unselect();
    //    }

    //    private void OnEntityDespawned(EntityDespawnedEvent e)
    //    {
    //        if (this.Selectable == e.Entity)
    //            this.Unselect();
    //        else
    //        {
    //            if (this.MultipleSelected.FirstOrDefault(t => t.Object == e.Entity) is TargetArgs t)
    //                this.MultipleSelected.Remove(t);
    //            if (this.MultipleSelected.Count == 0)
    //            {
    //                this.PanelInfo.Hide();
    //                this.BoxTabs.Hide();
    //            }
    //        }
    //    }
    //    void Unselect(TargetArgs t)
    //    {
    //        this.MultipleSelected.Remove(t);
    //        Renderer.Invalidate();
    //        this.BoxOrderButtons.ClearControls();
    //        foreach(var target in this.MultipleSelected)
    //            target.Map.Town.Select(target, this);
    //    }
    //    private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
    //    {
    //        if (this.Selectable == e.Entity)
    //            this.Unselect();
    //        else
    //        {
    //            //if (this.MultipleSelected.FirstOrDefault(t => t.BlockEntity == e.Entity) is TargetArgs t)
    //            if (this.MultipleSelected.FirstOrDefault(t =>
    //                t.BlockEntity == e.Entity ||
    //                t.Global == (Vector3)e.Entity.OriginGlobal) is TargetArgs t)
    //            {
    //                this.Unselect(t);
    //            }
    //            //else if(this.MultipleSelected.FirstOrDefault(t=> t.Global == (Vector3)e.Entity.OriginGlobal) is TargetArgs t)
    //            //    this.MultipleSelected.Remove(t);
    //            if (this.MultipleSelected.Count == 0)
    //            {
    //                this.PanelInfo.Hide();
    //                this.BoxTabs.Hide();
    //                this.BoxIcons.Hide();
    //                this.BoxOrderButtons.Hide();
    //            }
    //        }
    //    }
    //    private void OnBlockEntityUpdated(BlockEntityUpdatedEvent e)
    //    {
    //        if (this.Selectable == e.Entity)
    //            this.PanelInfo.Invalidate(true);
    //    }
    //    private void OnZoneDeleted(ZoneDeletedEvent e)
    //    {
    //        if (this.Selectable == e.Zone)
    //            this.Unselect();
    //    }
    //    void Unselect()
    //    {
    //        this.SelectSingle(TargetArgs.Null);
    //    }
    //    public TargetArgs SelectedSource = TargetArgs.Null;
    //    ISelectable Selectable;
    //    Window WindowInfo;
    //    IEnumerator<ISelectable> SelectedStack;
    //    public List<TargetArgs> MultipleSelected = []; // TODO: make this a list of iselectables

    //    SelectionManager()
    //    {
    //        this.PanelInfo = Panel.FromClientSize(302, Label.DefaultHeight * 6);// 100); (302 = fit 3 x 100px widt bars, width 1 px spacing between them
    //        this.BoxTabs = new GroupBox()
    //        {
    //            AutoSize = false,
    //            Size = new Rectangle(0, 0, this.PanelInfo.Width, Button.DefaultHeight)
    //        };
    //        this.BoxTabs.AnchorTo(() => this.PanelInfo.ScreenLocation, Vector2.UnitY);

    //        this.PanelInfo.AnchorToBottomCenter();
    //        this.LabelName = new Label() { TextFunc = () => "<none>" };
    //        Lazy<SelectionDetailsGui> detailsGui = new Lazy<SelectionDetailsGui>(() => new SelectionDetailsGui());
    //        this.IconDetails = new IconButton("^")
    //        {
    //            BackgroundTexture = UIManager.Icon16Background,
    //            LeftClickAction = () =>
    //            {
    //                detailsGui.Value.Refresh(Instance.SelectedSource ?? Instance.SelectedStack.Current).GetOrCreateWindow("Details").Toggle();
    //            },
    //            HoverText = "Details"
    //        };
    //        this.IconInfo = new IconButton("?")
    //        {
    //            BackgroundTexture = UIManager.Icon16Background,
    //            LeftClickAction = ToggleInfo,
    //            HoverText = "Inspect"
    //        };
    //        this.IconCenter = new IconButton(Icon.ArrowUp)
    //        {
    //            BackgroundTexture = UIManager.Icon16Background,
    //            LeftClickAction = CenterCamera,
    //            HoverText = "Center camera"
    //        };
    //        this.IconCycle = new IconButton(Icon.Replace)
    //        {
    //            BackgroundTexture = UIManager.Icon16Background,
    //            LeftClickAction = this.CycleTargets,
    //            HoverText = "Cycle targets"
    //        };

    //        this.IconIssues = new IconButton("!") { BackgroundTexture = UIManager.Icon16Background, TooltipFunc = showIssuesTooltip }
    //            .Flash(true)
    //            .VisibleWhen(() => SelectedBlockEntity?.Errors.Any() ?? false) as IconButton;

    //        static void showIssuesTooltip(Control tooltip)
    //        {
    //            if (SelectedBlockEntity is BlockEntity blentity)
    //                tooltip.AddControlsBottomLeft(blentity.GetErrorsGui());
    //        }

    //        this.BoxIcons = new GroupBox();
    //        this.PopulateBoxIcons();

    //        this.BoxOrderButtons = new GroupBox();
    //        this.BoxOrderButtons.BackgroundColorFunc = () => Color.Black * .5f;
    //        this.BoxOrderButtons.LocationFunc = () => this.PanelInfo.BottomRight;
    //        this.BoxOrderButtons.Anchor = new Vector2(0, 1);
    //        this.BoxOrderButtons.ControlsChangedAction = () => this.BoxOrderButtons.AlignLeftToRight();

    //        this.BoxInfo = new GroupBox() { Location = this.LabelName.BottomLeft };
    //        this.PanelInfo.AddControls(
    //            this.LabelName,
    //            this.BoxIcons,
    //            this.BoxInfo
    //            );

    //    }

    //    private void RepositionsBoxIcons()
    //    {
    //        this.BoxIcons.AlignLeftToRight();
    //        this.BoxIcons.Location = new Vector2(this.PanelInfo.ClientSize.Right, this.PanelInfo.ClientSize.Top);
    //        this.BoxIcons.Anchor = new Vector2(1, 0);
    //    }

    //    private void PopulateBoxIcons()
    //    {
    //        this.BoxIcons.ClearControls();
    //        this.BoxIcons.AddControls(
    //            IconIssues,
    //            IconSlice,
    //            this.IconCenter,
    //            this.IconInfo,
    //            this.IconDetails
    //            );

    //        if (this.SelectedStack != null)
    //            this.BoxIcons.AddControls(this.IconCycle);

    //        this.RepositionsBoxIcons();
    //    }

    //    private void CenterCamera()
    //    {
    //        if (this.SelectedSource != null)
    //            if (this.SelectedSource.Type != TargetType.Null)
    //                ScreenManager.CurrentScreen.Camera.CenterOn(this.SelectedSource.Global);
    //    }
    //    public void SetName(string text)
    //    {
    //        this.LabelName.TextFunc = () => text;
    //    }
    //    private static void ToggleInfo()
    //    {
    //        if (Instance.SelectedSource.Object is Inspectable obj)
    //            Inspector.Refresh(obj);
    //        else
    //        {
    //            if (Instance.SelectedStack.Current is Inspectable insp)
    //                Inspector.Refresh(insp);
    //            else
    //                Inspector.Refresh(Instance.SelectedSource);
    //        }
    //        Inspector.Show();
    //    }

    //    public static void Select(TargetArgs target)
    //    {
    //        Instance.SelectSingle(target);
    //    }
    //    public static void Select(MapBase map, BoundingBox box)
    //    {
    //        Select(map.GetObjects(box).Select(s => new TargetArgs(s)));
    //    }
    //    public static void Select(MapBase map, IntVec3 begin, IntVec3 end)
    //    {
    //        var box = new BoundingBox(begin, end).ToListIntVec3();
    //        var cells = box.Select(c => new TargetArgs(map, c)).Where(t => t.Cell.Block != BlockDefOf.Air.Worker || t.BlockEntityOld is not null);
    //        Select(cells);
    //    }
    //    public static void Select(IEnumerable<GameObject> entities)
    //    {
    //        Select(entities.Select(e => new TargetArgs(e)));
    //    }
    //    public static void Select(IEnumerable<TargetArgs> targets)
    //    {
    //        Instance.SelectInternal(targets);
    //    }
    //    /// <summary>
    //    /// why did i have this commented out?
    //    /// because it doesn't set the map field in targetargs
    //    /// </summary>
    //    /// <param name="cells"></param>
    //    public static void Select(MapBase map, IEnumerable<IntVec3> cells)
    //    {
    //        Instance.SelectInternal(cells.Select(c => c.At(map)));
    //    }

    //    internal static void OnCameraRotated(Camera camera)
    //    {
    //        Renderer.Invalidate();
    //    }

    //    internal static void SelectAllVisible(ItemDef def)
    //    {
    //        var objects = Ingame.Instance.Scene.ObjectsDrawn.Where(i => i.Def == def).Select(o => new TargetArgs(o));
    //        Select(objects);
    //    }
    //    internal static void AddToSelection(IEnumerable<GameObject> targets)
    //    {
    //        AddToSelection(targets.Select(o => new TargetArgs(o)));
    //    }
    //    internal static void AddToSelection(IEnumerable<TargetArgs> targets)
    //    {
    //        var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2.IsEqual(t))).Concat(targets).ToList();
    //        Instance.SelectInternal(list);
    //    }
    //    internal static void AddToSelection(TargetArgs target)
    //    {
    //        var existing = Instance.MultipleSelected.FirstOrDefault(t => t.IsEqual(target));
    //        if (existing != null)
    //            Instance.SelectInternal(Instance.MultipleSelected.Except([existing]));
    //        else
    //            Instance.SelectInternal([.. Instance.MultipleSelected, target]);
    //    }
    //    private static IEnumerable<TargetArgs> FilterActors(IEnumerable<TargetArgs> targets)
    //    {
    //        if (targets.Any(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>()))
    //            return targets.Where(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>());
    //        return targets;
    //    }

    //    private void SelectInternal(IEnumerable<TargetArgs> targets)
    //    {
    //        this.SelectSingle(TargetArgs.Null);
    //        this.MultipleSelected = [.. FilterActors(targets).Where(t => t.Exists)];
    //        if (this.MultipleSelected.Count == 0)
    //            return;
    //        if (this.MultipleSelected.Count == 1)
    //        {
    //            this.SelectSingle(targets.First());
    //            return;
    //        }

    //        this.LabelName.TextFunc = () => $"Multiple x{this.MultipleSelected.Count}";

    //        this.CreateButtons(targets);
    //        this.PanelInfo.RemoveControls(this.BoxIcons);
    //        this.Show();
    //    }
    //    private void SelectSingle(TargetArgs target)
    //    {
    //        Renderer.Invalidate();

    //        if (this.SelectedSource.IsEqual(target))
    //        {
    //            this.CycleTargets();
    //            return;
    //        }
    //        this.SelectedSource = target;
    //        this.SelectedStack = null;
    //        this.WindowInfo = null;
    //        this.Clear();
    //        switch (target.Type)
    //        {
    //            case TargetType.Entity:
    //                var entity = target.Object;
    //                this.LabelName.TextFunc = () => entity.Name;
    //                this.MultipleSelected.Clear();
    //                this.MultipleSelected.Add(target);
    //                entity.GetSelectionInfo(this);
    //                entity.GetQuickButtons(this);
    //                this.InitInfoTabs(entity.GetQuickButtons(), target);
    //                entity.Map?.Town?.Select(target, this);
    //                this.InitInfoTabs(entity.Map?.Town?.GetTabs(target));
    //                break;

    //            case TargetType.Position:
    //                this.MultipleSelected.Clear();
    //                this.MultipleSelected.Add(target);
    //                var selectables = target.Map.Town.QuerySelectables(target);
    //                this.SelectedStack = selectables.GetEnumerator();
    //                this.CycleTargets();
    //                if (target.Map.IsUndiscovered(target.Global))
    //                    this.LabelName.TextFunc = () => "Unknown block";
    //                break;

    //            case TargetType.BlockEntity:
    //                this.MultipleSelected.Clear();
    //                this.MultipleSelected.Add(target);
    //                this.SetName(target.Name);

    //                target.GetSelectionInfo(this);
    //                target.GetQuickButtons(this);
    //                this.InitInfoTabs(target.GetInfoTabs());
    //                target.Map.Town.Select(target, this);
    //                break;

    //            case TargetType.Null:
    //                this.PanelInfo.Hide();
    //                this.BoxOrderButtons.Hide();
    //                this.LabelName.TextFunc = () => "<none>";
    //                if (this.WindowInfo != null)
    //                    this.WindowInfo.Hide();
    //                this.WindowInfo = null;
    //                this.SelectedSource = TargetArgs.Null;
    //                this.Selectable = null;
    //                this.MultipleSelected.Clear();
    //                return;

    //            default:
    //                break;
    //        }
    //        this.SelectedSource = target;
    //        this.Show();

    //        this.PanelInfo.WindowManager.OnSelectedTargetChanged(target);
    //        this.PanelInfo.Validate(true);
    //    }
    //    void Show()
    //    {
    //        this.BoxTabs.Show();
    //        this.PanelInfo.Show();
    //        this.BoxOrderButtons.Show();
    //    }

    //    void Hide()
    //    {
    //        this.BoxTabs.Hide();
    //        this.PanelInfo.Hide();
    //        this.BoxOrderButtons.Hide();
    //    }
    //    private void Clear()
    //    {
    //        foreach (var a in this.ActionsAdded)
    //            a.Value.Clear();
    //        this.ActionsAdded.Clear();
    //        this.BoxTabs.ClearControls();
    //        this.BoxOrderButtons.ClearControls();
    //        this.BoxInfo.ClearControls();
    //        this.PanelInfo.ClearControls();
    //        this.PopulateBoxIcons();
    //        this.PanelInfo.AddControls(
    //            this.LabelName,
    //            this.BoxInfo,
    //            this.BoxIcons);
    //    }

    //    private void CycleTargets()
    //    {
    //        if (this.SelectedStack == null)
    //            return;
    //        this.SelectedStack.MoveNext();
    //        var first = this.SelectedStack.Current;
    //        this.SetName(first.Name);
    //        this.Clear();

    //        first.GetSelectionInfo(this);
    //        first.GetQuickButtons(this);
    //        this.InitInfoTabs(first.GetInfoTabs());
    //        Client.Instance.Map.Town.Select(first, this);
    //        this.Selectable = first;
    //    }
    //    void InitInfoTabs(IEnumerable<(string, Type)> tabs, ISelectable selectable)
    //    {
    //        foreach (var (label, guiType) in tabs)
    //            this.AddTabAction(label, () => UIManager.ToggleSingleton(guiType, selectable), Color.Orange);
    //            //this.AddTabAction(label, () => UIManager.ToggleUnique(guiType, selectable), Color.Orange);
    //    }
    //    void InitInfoTabs(IEnumerable<(string name, Action action)> tabs)
    //    {
    //        foreach (var (name, action) in tabs)
    //            this.AddTabAction(name, action, Color.Orange);
    //    }
    //    void InitInfoTabs(IEnumerable<Button> tabs)
    //    {
    //        if (tabs is null)
    //            return;
    //        foreach (var button in tabs)
    //            this.AddTabAction(button);
    //    }
    //    internal static bool IsSelected(ISelectable item)
    //    {
    //        if (Instance.SelectedStack == null)
    //            return false;
    //        return Instance.SelectedStack.Current == item;
    //    }

    //    readonly Dictionary<Action<List<TargetArgs>>, List<TargetArgs>> ActionsAdded = new();

    //    private void CreateButtons(IEnumerable<TargetArgs> targets)
    //    {
    //        this.BoxOrderButtons.ClearControls();
    //        this.ActionsAdded.Clear();
    //        foreach (var tar in targets)
    //            tar.GetQuickButtons(this);
    //        Client.Instance.Map.Town.Select(null, this);
    //        this.UpdateOrderButtons();
    //    }

    //    void AddTabAction(Button button)
    //    {
    //        button.BackgroundColor = UIManager.TintPrimary * .5f;
    //        this.BoxTabs.AddControlsLineWrap([button], this.PanelInfo.Width);
    //    }
    //    void AddTabAction(string label, Action action, Color col)
    //    {
    //        this.BoxTabs.AddControlsLineWrap([new Button(label) { LeftClickAction = action, BackgroundColor = col * .5f }], this.PanelInfo.Width);
    //    }
    //    public void AddTabAction(string label, Action action)
    //    {
    //        this.AddTabAction(label, action, Color.PaleVioletRed);
    //    }
    //    internal void AddTabs(params Button[] buttons)
    //    {
    //        this.BoxTabs.AddControls(buttons);
    //    }

    //    internal static void AddButton(IconButton button)
    //    {
    //        Instance.AddButtons(new IconButton[] { button });
    //    }
    //    private void MultipleSelectedAction(Action<List<TargetArgs>> action)
    //    {
    //        action(this.ActionsAdded[action]);
    //    }

    //    public void Update()
    //    {
    //        /// move this to ongameevent?
    //        if (this.SelectedSource is not null && this.SelectedSource.Type == TargetType.Entity && this.SelectedSource.Object.IsDisposed)
    //            this.SelectSingle(TargetArgs.Null);

    //        if (this.Selectable is null)
    //        {
    //            if (!this.MultipleSelected.Any())
    //                if (this.PanelInfo.IsOpen)
    //                    this.Hide();
    //            return;
    //        }

    //        /// do i really need this? i handle the blockschanged message anyway, and this causes problems for selecting undiscovered air blocks 
    //        if (!this.Selectable.Exists)
    //            this.SelectSingle(TargetArgs.Null);
    //    }
    //    public void DrawWorld(MySpriteBatch sb, Camera camera)
    //    {
    //        if (this.MultipleSelected.Count != 0)
    //        {

    //            var first = this.MultipleSelected.First();
    //            var map = first.Map;
    //            if (first.Type == TargetType.Position)
    //                Renderer.DrawBlocks(map, camera, this.MultipleSelected.Select(t => (IntVec3)t.Global));
    //            else if(first.Type == TargetType.BlockEntity)
    //            {
    //                Renderer.DrawBlocks(map, camera, first.BlockEntity.CellsOccupied);

    //                // HACK: using this to make the origin block draw its interaction spot, if any
    //                map.GetBlock(first.BlockEntity.OriginGlobal).DrawSelected(sb, camera, map, first.BlockEntity.OriginGlobal);
    //            }
    //            if (SingleSelectedCell.HasValue)
    //            {
    //                var singleCell = SingleSelectedCell.Value;
    //                map.GetBlock(singleCell).DrawSelected(sb, camera, map, singleCell);
    //            }
    //        }
    //        else if (this.SelectedSource != null) // this block never executes aymore because even single selections are contained in the multipleselected collection
    //        {
    //            if (this.SelectedSource.Type == TargetType.Position)
    //            {
    //                camera.DrawCellHighlights(Block.BlockHighlight, [this.SelectedSource.Global], Color.Yellow);
    //                var map = this.SelectedSource.Map;
    //                var global = this.SelectedSource.Global;
    //                map.GetBlock(global).DrawSelected(sb, camera, map, global);
    //            }
    //            if(this.SelectedSource.Type == TargetType.BlockEntitySlot)
    //            {

    //            }
    //        }
    //    }
    //    public void DrawOnCamera(SpriteBatch sb, Camera camera)
    //    {
    //        if (this.MultipleSelected.Any())
    //            foreach (var obj in this.MultipleSelected)
    //                if (obj.Type == TargetType.Entity && obj.Object.Map == Ingame.CurrentMap)
    //                    obj.Object.DrawBorder(sb, camera);
    //                else if (this.SelectedSource != null)
    //                    if (this.SelectedSource.Type == TargetType.Entity)
    //                        this.SelectedSource.Object.DrawBorder(sb, camera);
    //    }

    //    public static bool IsSelected(TargetArgs tar)
    //    {
    //        return Instance.MultipleSelected.Any(t => t.IsEqual(tar)) || Instance.SelectedSource.IsEqual(tar);
    //    }
    //    public static bool IsSelected(IntVec3 tar)
    //    {
    //        return
    //            Instance.MultipleSelected.Any(t => t.Type == TargetType.Position && (IntVec3)t.Global == tar) ||
    //            Instance.SelectedSource.Type == TargetType.Position && (IntVec3)Instance.SelectedSource.Global == tar;
    //    }
    //    internal static bool ClearTargets()
    //    {
    //        if (!Instance.MultipleSelected.Any() && Instance.SelectedSource.Type == TargetType.Null)
    //            return false;
    //        Instance.SelectSingle(TargetArgs.Null);
    //        return true;
    //    }

    //    public void AddIcon(IconButton icon)
    //    {
    //        if (this.MultipleSelected.Count > 1)
    //            return;

    //        this.BoxIcons.Controls.Insert(0, icon);
    //        this.RepositionsBoxIcons();
    //    }

    //    public void AddInfo(Control ctrl)
    //    {
    //        this.BoxInfo.AddControls(ctrl);
    //        this.BoxInfo.Controls.AlignVertically();
    //    }
    //    public static void RemoveInfo(Control ctrl)
    //    {
    //        Instance.BoxInfo.RemoveControls(ctrl);
    //    }
    //    public static void AddInfoNew(Control ctrl)
    //    {
    //        Instance.BoxInfo.AddControls(ctrl);
    //    }
    //    public static void RemoveOrderButton(IconButton button)
    //    {
    //        Instance.BoxOrderButtons.RemoveControls(button);
    //    }

    //    internal void AddButtons(params IconButton[] buttons)
    //    {
    //        this.BoxOrderButtons.AddControls(buttons);
    //    }
    //    internal void AddButton(IconButton button, Action<List<TargetArgs>> action, GameObject obj, bool singleTargetOnly = false)
    //    {
    //        this.AddButton(button, action, new TargetArgs(obj), singleTargetOnly);
    //    }
    //    internal void AddButton(IconButton button, Action<TargetArgs> action, TargetArgs target)
    //    {
    //        this.AddButton(button, targets => action(targets.First()), target, true);
    //    }
    //    internal void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs obj, bool singleTargetOnly = false)
    //    {
    //        if (singleTargetOnly && this.MultipleSelected.Count > 1)
    //            return;

    //        if (this.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
    //        {
    //            existing.Add(obj);
    //            return;
    //        }
    //        else
    //            this.ActionsAdded.Add(action, new List<TargetArgs>() { obj });
    //        button.LeftClickAction = () => this.MultipleSelectedAction(action);
    //        this.BoxOrderButtons.AddControls(button);
    //    }
    //    internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<GameObject> targets)
    //    {
    //        AddOrderButton(button, action, targets.Select(t => new TargetArgs(t)));
    //    }
    //    internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<IntVec3> cells)
    //    {
    //        AddOrderButton(button, action, cells.Select(t => t.At(Client.Instance.Map)));
    //    }
    //    internal static void AddOrderButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<TargetArgs> targets)
    //    {
    //        if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
    //        {
    //            Instance.ActionsAdded.Remove(action);
    //        }
    //        Instance.ActionsAdded.Add(action, targets.ToList());
    //        if (!Instance.BoxOrderButtons.Controls.Contains(button))
    //            Instance.BoxOrderButtons.AddControls(button);
    //        button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
    //    }
    //    SelectionIntent CurrentSelection;
    //    internal void Select(SelectionIntent selection)
    //    {
    //        this.CurrentSelection = selection;
    //        Select(selection.ResolveTargets(Ingame.GetMap()));
    //    }
    //    void UpdateOrderButtons()
    //    {
    //        var map = Ingame.GetMap(); // temp
    //        var targets = this.CurrentSelection.ResolveTargets(map);
    //        foreach (var orderdef in AllOrderCommands.Value)
    //        {
    //            if (targets.Any(orderdef.Worker.CanIssue))
    //            {
    //                var runtime = new OrderCommandRuntime(orderdef);
    //                var button = new QuickButton(new Icon(orderdef.Sprite), null, orderdef.LabelReadable)
    //                {
    //                    LeftClickAction = () => runtime.Issue(this.CurrentSelection)
    //                };
    //                this.BoxOrderButtons.AddControls(button);
    //            }
    //        }
    //    }
    //    //void UpdateOrderButtons()
    //    //{
    //    //    var selected = this.MultipleSelected;
    //    //    foreach (var orderdef in AllOrderCommands.Value)
    //    //    {
    //    //        if (selected.Any(orderdef.Worker.CanIssue))
    //    //        {
    //    //            var runtime = new OrderCommandRuntime(orderdef);
    //    //            var button = new QuickButton(new Icon(orderdef.Sprite), null, orderdef.LabelReadable)
    //    //            {
    //    //                LeftClickAction = () => runtime.Issue(selected)
    //    //            };
    //    //            this.BoxOrderButtons.AddControls(button);
    //    //        }
    //    //    }
    //    //}
    //    internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs target)
    //    {
    //        if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
    //        {
    //            existing.Add(target);
    //        }
    //        else
    //        {
    //            Instance.ActionsAdded.Add(action, [target]);
    //            Instance.BoxOrderButtons.AddControls(button);
    //            button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
    //        }
    //    }
    //    internal static IEnumerable<TargetArgs> Selected => Instance.MultipleSelected;

    //    internal static IEnumerable<GameObject> GetSelectedEntities()
    //    {
    //        return Selected
    //            .Where(tar => tar.Type == TargetType.Entity)
    //            .Select(t => t.Object);
    //    }
    //    internal IEnumerable<IntVec3> GetSelectedCells()
    //    {
    //        //if (SingleSelectedCell.HasValue)
    //        //    return [SingleSelectedCell.Value];
    //        return this.MultipleSelected
    //            .Where(tar => tar.Type == TargetType.Position)
    //            .Select(t => (IntVec3)t.Global);
    //        return Selected
    //            .Where(tar => tar.Type == TargetType.Position)
    //            .Select(t => (IntVec3)t.Global);
    //    }




    //    internal static IEnumerable<IntVec3> SelectedCells => Instance.GetSelectedCells();
    //    internal static IEnumerable<GameObject> SelectedEntities => GetSelectedEntities();

    //    internal static TargetArgs SingleSelected => Instance.MultipleSelected.Count == 1 ? Instance.MultipleSelected.Single() : null;
    //    internal static Entity SingleSelectedEntity => SingleSelected?.Object as Entity;
    //    internal static IntVec3? SingleSelectedCell => (SingleSelected is TargetArgs target && target.Type == TargetType.Position) ? target.Global : null;
    //    internal static BlockEntity SelectedBlockEntity => (SingleSelected is TargetArgs target && target.Type == TargetType.Position) ? target.BlockEntityOld : null;
    //}
}
