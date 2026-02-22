using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Input;
using Project1.Core.Input.CellRendering;
using Project1.Core.Input.Orders;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Towns.Designations;
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
        readonly Dictionary<Action<List<ISelectable>>, List<ISelectable>> ActionsAdded = [];
        static readonly Lazy<List<OrderCommandDef>> AllOrderCommands = new(() => [.. Def.GetDefs<OrderCommandDef>()]);
        public ISelectable SelectedSource;
        ISelectable Selectable;
        Window WindowInfo;
        IReadOnlyList<ISelectable> SelectedStackNew;
        ISelectable SelectedStackCurrent => this.SelectedStackNew?[this._selectedStackIndex];
        int _selectedStackIndex;
        HashSet<ISelectable> MultipleSelected => this.Selection.Targets;
        readonly SelectionFinal Selection = new();
        public IReadOnlyCollection<ISelectable> CurrentSelections => this.Selection.Targets;
        static SelectionManager()
        {
            //Registry.MapEventHooksClient.Register<PlayerExecutedOrderCommentEvent>(OnPlayerExecutedOrderCommand);
        }
        //private static void OnPlayerExecutedOrderCommand(PlayerExecutedOrderCommentEvent e)
        //{
        //    if (e.Map != Ingame.CurrentMap)
        //        return;
        //    Instance.RefreshOrderButtons();
        //}

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

            map.Events.ListenTo<DesignationsChangedEvent>(OnDesignationsChanged);
        }

        private void OnDesignationsChanged(DesignationsChangedEvent e)
        {
            this.RefreshOrderButtons();
        }

        internal void Init(Ingame ingame)
        {
            ingame.Events.ListenTo<PlayerSelectionSingleEvent>(OnPlayerSelectionSingle);
            ingame.Events.ListenTo<PlayerSelectionCubeEvent>(OnPlayerSelectionCube);
            ingame.Events.ListenTo<PlayerSelectionRectangleEvent>(OnPlayerSelectionRectangle);
        }
        private void OnPlayerSelectionRectangle(PlayerSelectionRectangleEvent e)
        {
            switch (e.SelectionOp)
            {
                case SelectionOp.Clear:
                    this.SelectInternal(e.Entities);
                    break;
                case SelectionOp.Add:
                    this.SelectInternal(this.Selection.Targets.OfType<Entity>().ToArray().Union(e.Entities));
                    break;
                case SelectionOp.Remove:
                    this.SelectInternal(this.Selection.Targets.OfType<Entity>().ToArray().Except(e.Entities));
                    break;
                default:
                    throw new UnreachableException();
            }
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
        private void OnPlayerSelectionSingle(PlayerSelectionSingleEvent e)
        {
            this.Select(e.Single);
        }
        public void Select(TargetArgs target)
        {
            ISelectable selectable = target.Type switch
            {
                TargetType.Entity => target.Entity,
                TargetType.BlockEntity => target.BlockEntity,
                TargetType.Cell => new CellSelection(target.Map, target.Global, target.Face),
                _ => throw new UnreachableException()
            };
            this.SelectSingle(selectable);
        }
        private void OnBlocksUpdated(CellsInvalidatedEvent e)
        {
            if (this.Selectable is CellSelection cell && e.Positions.Contains(cell.Global))
                this.Unselect();
        }
        private void OnEntityDespawned(EntityDespawnedEvent e)
        {
            if (this.Selectable == e.Entity)
                this.Unselect();
            else
            {
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
        private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (this.Selectable == e.Entity)
                this.Unselect();
            else
            {
                if (this.MultipleSelected.FirstOrDefault(t =>
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
            this.SelectSingle(null);
        }
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
            else if (Instance.SelectedStackCurrent is Inspectable insp)
                    Inspector.Refresh(insp);
            Inspector.Show();
        }
        public static void Select(MapBase map, BoundingBox box)
        {
            Select(map.GetObjects(box).Select(s => new TargetArgs(s)));
        }
        private void Select(IntVec3 begin, IntVec3 end)
        {
            this.SelectedStackNew = null;
            this.Selection.SetBox(Ingame.CurrentMap, begin, end);
            this.Renderer.Invalidate();
            this.LabelName.TextFunc = () => $"Multiple cells x{this.Selection.Targets.Count}";
            this.RefreshOrderButtons();
        }
        public static void Select(IEnumerable<TargetArgs> targets)
        {
            Instance.SelectInternal(targets);
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
        internal static void AddToSelection(IEnumerable<ISelectable> targets)
        {
            var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2 == t)).Concat(targets).ToList();
            Instance.SelectInternal(list);
        }
        private static IEnumerable<ISelectable> FilterActors(IEnumerable<ISelectable> targets)
        {
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
            this.SelectedStackNew = null;
            this.WindowInfo = null;
            this.Clear();
            switch (target)
            {
                case Entity entity:
                    this.LabelName.TextFunc = () => entity.Name;
                    this.Selection.Add(target);
                    entity.GetSelectionInfo(this);
                    entity.GetQuickButtons(this);
                    this.InitInfoTabs(entity.GetQuickButtons(), target);
                    entity.Map?.Town?.Select(target, this);
                    this.InitInfoTabs(entity.Map?.Town?.GetTabs(target));
                    break;

                case CellSelection cell:
                    this.Selection.Add(target);
                    this.SelectedStackNew = cell.Map.Town.QuerySelectablesNew(cell);
                    //this._selectedStackIndex = 0;
                    this.CycleTargetsNew();
                    if (cell.Map.IsUndiscovered(cell.Global))
                        this.LabelName.TextFunc = () => "Unknown block";
                    break;

                case BlockEntity blockEntity:
                    this.Selection.Add(target);

                    this.SetName(target.Name);

                    target.GetSelectionInfo(this);
                    target.GetQuickButtons(this);
                    this.InitInfoTabs(target.GetInfoTabs());
                    blockEntity.Map.Town.Select(target, this);
                    break;

                case null:
                    this.PanelInfo.Hide();
                    this.BoxOrderButtons.Hide();
                    this.LabelName.TextFunc = () => "<none>";
                    this.WindowInfo?.Hide();
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
        internal static void AddButton(IconButton button)
        {
            Instance.AddButtons([button]);
        }
        private void MultipleSelectedAction(Action<List<ISelectable>> action)
        {
            action(this.ActionsAdded[action]);
        }
        public void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            var map = Ingame.GetMap();

            if (this.Selection.Cells.Any())
                this.Renderer.DrawBlocks(map, camera, this.Selection.Cells);
            else
            {
                foreach (var blockEntity in this.Selection.Targets.OfType<BlockEntity>())
                {
                    Renderer.DrawBlocks(map, camera, blockEntity.CellsOccupied);
                    // HACK: using this to make the origin block draw its interaction spot, if any
                    map.GetBlock(blockEntity.OriginGlobal).DrawSelected(sb, camera, map, blockEntity.OriginGlobal);
                }
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
        }
        internal static bool IsSelected(ISelectable item)
        {
            return Instance.SelectedStackCurrent == item;
        }
        internal static bool ClearTargets()
        {
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
        internal static void AddOrderButton(IconButton button, Action<List<ISelectable>> action, IEnumerable<ISelectable> targets)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out var existing))
                Instance.ActionsAdded.Remove(action);
            Instance.ActionsAdded.Add(action, targets.ToList());
            if (!Instance.BoxOrderButtons.Controls.Contains(button))
                Instance.BoxOrderButtons.AddControls(button);
            button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
        }
        internal void RefreshOrderButtons()
        {
            this.BoxOrderButtons.ClearControls();
            var map = Ingame.CurrentMap;
            var targets = this.SelectedStackCurrent is not null ? [this.SelectedStackCurrent] : this.CurrentSelections;
            foreach (var orderdef in AllOrderCommands.Value)
            {
                if (orderdef.Worker.CanIssue(targets))// targets.Any(orderdef.Worker.CanIssue))
                {
                    var runtime = new OrderCommandRuntime(orderdef);
                    var button = new QuickButton(new Icon(orderdef.Sprite), null, orderdef.LabelReadable)
                    {
                        LeftClickAction = () => runtime.Issue(this.Selection)//.ToSelectionIntent())
                    };
                    this.BoxOrderButtons.AddControls(button);
                }
            }
            if (this.BoxOrderButtons.Controls.Count == 0)
                this.BoxOrderButtons.Hide();
        }
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
        internal static IEnumerable<Entity> GetSelectedEntities()
        {
            return Instance.CurrentSelections
                .OfType<Entity>();
        }
        internal IEnumerable<CellSelection> GetSelectedCells()
        {
            return this.CurrentSelections.OfType<CellSelection>();
        }
        internal static IEnumerable<CellSelection> SelectedCells => Instance.GetSelectedCells();
        internal static IEnumerable<GameObject> SelectedEntities => GetSelectedEntities();
        internal static ISelectable SingleSelected => Instance.MultipleSelected.Count == 1 ? Instance.MultipleSelected.Single() : null;
        internal static Entity SingleSelectedEntity => SingleSelected as Entity;
        internal static IntVec3? SingleSelectedCell => (SingleSelected is TargetArgs target && target.Type == TargetType.Cell) ? target.Global : null;
        internal static BlockEntity SelectedBlockEntity => (SingleSelected is TargetArgs target && target.Type == TargetType.Cell) ? target.BlockEntityOld : null;
    }
}
