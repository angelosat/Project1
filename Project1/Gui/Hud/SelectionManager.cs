using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    //[EnsureStaticCtorCall]
    public sealed class SelectionManager : GroupBox, IUISelection
    {
        readonly GroupBox BoxTabs, BoxButtons, BoxIcons, BoxInfo;
        public Panel PanelInfo;
        public Label LabelName;
        readonly IconButton IconInfo, IconCenter, IconDetails;
        readonly IconButton IconCycle;
        readonly IconButton IconIssues;
        static readonly BlockRendererNew Renderer = new(Block.BlockHighlight);
        static SelectionManager()
        {
        }
        static readonly IconButton IconSlice = new(Icon.ArrowDown)
        {
            BackgroundTexture = UIManager.Icon16Background,
            LeftClickAction = ToolManagement.Slice,
            HoverText = "Slice z-level"
        };
        public static SelectionManager Instance = new();

        public TargetArgs SelectedSource = TargetArgs.Null;
        ISelectable Selectable;
        Window WindowInfo;
        IEnumerator<ISelectable> SelectedStack;
        public List<TargetArgs> MultipleSelected = new(); // TODO: make this a list of iselectables

        SelectionManager()
        {
            //this.PanelInfo = new Panel(new Rectangle(0, 0, 300, 100));
            this.PanelInfo = Panel.FromClientSize(302, Label.DefaultHeight * 6);// 100); (302 = fit 3 x 100px widt bars, width 1 px spacing between them
            this.BoxTabs = new GroupBox()
            {
                AutoSize = false,
                //Size = new Rectangle(0, 0, 300, Button.DefaultHeight)
                Size = new Rectangle(0, 0, this.PanelInfo.Width, Button.DefaultHeight)
            };
            this.PanelInfo.Location = this.BoxTabs.BottomLeft;
            this.LabelName = new Label() { TextFunc = () => "<none>" };
            Lazy<SelectionDetailsGui> detailsGui = new Lazy<SelectionDetailsGui>(() => new SelectionDetailsGui());
            this.IconDetails = new IconButton("^")
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = () =>
                {
                    //detailsGui.Value.Refresh(Instance.SelectedSource ?? Instance.SelectedStack.Current);
                    detailsGui.Value.Refresh(Instance.SelectedSource ?? Instance.SelectedStack.Current).GetOrCreateWindow("Details").Toggle();
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
                LeftClickAction = this.CycleTargets,
                HoverText = "Cycle targets"
            };

            this.IconIssues = new IconButton("!") { BackgroundTexture = UIManager.Icon16Background, TooltipFunc = showIssuesTooltip }
                .Flash(true)
                .VisibleWhen(() => SelectedBlockEntity?.Errors.Any() ?? false) as IconButton;

            static void showIssuesTooltip(Control tooltip)
            {
                if(SelectedBlockEntity is BlockEntity blentity)
                    tooltip.AddControlsBottomLeft(blentity.GetErrorsGui());
            }

            this.BoxIcons = new GroupBox();
            this.PopulateBoxIcons();

            this.BoxButtons = new GroupBox();
            this.BoxButtons.BackgroundColorFunc = () => Color.Black * .5f;
            this.BoxButtons.LocationFunc = () => this.BottomRight;
            this.BoxButtons.Anchor = new Vector2(0, 1);
            this.BoxButtons.ControlsChangedAction = () => this.BoxButtons.AlignLeftToRight();

            this.BoxInfo = new GroupBox() { Location = this.LabelName.BottomLeft };
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxIcons,
                this.BoxInfo
                );

            this.AddControls(
                this.BoxTabs,
                this.PanelInfo
                );
        }

        private void RepositionsBoxIcons()
        {
            this.BoxIcons.AlignLeftToRight();
            //this.BoxIcons.Location = new Vector2(this.PanelInfo.ClientSize.Right - Panel.DefaultStyle.Border, this.PanelInfo.ClientSize.Top);
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

            if (this.SelectedStack != null)
                this.BoxIcons.AddControls(this.IconCycle);

            this.RepositionsBoxIcons();
        }

        private void CenterCamera()
        {
            if (this.SelectedSource != null)
                if (this.SelectedSource.Type != TargetType.Null)
                    ScreenManager.CurrentScreen.Camera.CenterOn(this.SelectedSource.Global);// .CenterOn(this.SelectedSource.Global);
        }
        public void SetName(string text)
        {
            this.LabelName.TextFunc = () => text;
        }
        private static void ToggleInfo()
        {
            if (Instance.SelectedSource.Object is Inspectable obj)
                Inspector.Refresh(obj);
            else
            {
                if (Instance.SelectedStack.Current is Inspectable insp)
                    Inspector.Refresh(insp);
                else
                    Inspector.Refresh(Instance.SelectedSource);
            }
            Inspector.Show();
        }

        public static void Select(TargetArgs target)
        {
            Instance.SelectInternal(target);
        }
        public static void Select(MapBase map, BoundingBox box)
        {
            Select(map.GetObjects(box).Select(s => new TargetArgs(s)));
        }
        public static void Select(IEnumerable<GameObject> entities)
        {
            Select(entities.Select(e => new TargetArgs(e)));
        }
        public static void Select(IEnumerable<TargetArgs> targets)
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
            Instance.SelectInternal(cells.Select(c => new TargetArgs(map, c)));
        }

        internal static void OnCameraRotated(Camera camera)
        {
            Renderer.Invalidate();
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
        internal static void AddToSelection(IEnumerable<TargetArgs> targets)
        {
            var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2.IsEqual(t))).Concat(targets).ToList();
            Instance.SelectInternal(list);
        }
        internal static void AddToSelection(TargetArgs target)
        {
            var existing = Instance.MultipleSelected.FirstOrDefault(t => t.IsEqual(target));
            if (existing != null)
                Instance.SelectInternal(Instance.MultipleSelected.Except(new TargetArgs[] { existing }));
            else
                Instance.SelectInternal(Instance.MultipleSelected.Concat(new TargetArgs[] { target }).ToList());
        }
        private IEnumerable<TargetArgs> FilterActors(IEnumerable<TargetArgs> targets)
        {
            if (targets.Any(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>()))
                return targets.Where(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>());
            return targets;
        }

        private void SelectInternal(IEnumerable<TargetArgs> targets)
        {
            this.SelectInternal(TargetArgs.Null);
            this.MultipleSelected = this.FilterActors(targets).Where(t => t.Exists).ToList();
            if (this.MultipleSelected.Count == 0)
                return;
            if (this.MultipleSelected.Count == 1)
            {
                this.SelectInternal(targets.First());
                return;
            }

            this.LabelName.TextFunc = () => $"Multiple x{this.MultipleSelected.Count}";

            this.CreateButtons(targets);
            this.PanelInfo.RemoveControls(this.BoxIcons);
            this.Show();
        }
        private void SelectInternal(TargetArgs target)
        {
            Renderer.Invalidate();

            if (this.SelectedSource.IsEqual(target))
            {
                this.CycleTargets();
                return;
            }
            this.SelectedSource = target;
            this.SelectedStack = null;
            this.WindowInfo = null;
            this.Clear();
            switch (target.Type)
            {
                case TargetType.Entity:
                    var entity = target.Object;
                    this.LabelName.TextFunc = () => entity.GetName();
                    //this.MultipleSelected = new TargetArgs[] { target };
                    this.MultipleSelected.Clear();
                    this.MultipleSelected.Add(target);
                    entity.GetSelectionInfo(this);
                    entity.GetQuickButtons(this);
                    this.InitInfoTabs(entity.GetTabs());
                    entity.Town.Select(target, this);
                    this.InitInfoTabs(entity.Town.GetTabs(target));
                    break;

                case TargetType.Position:
                    //this.MultipleSelected = new TargetArgs[1] { target };
                    this.MultipleSelected.Clear();
                    this.MultipleSelected.Add(target);
                    var selectables = target.Map.Town.QuerySelectables(target);
                    if (selectables.Any())
                    {
                        this.SelectedStack = selectables.GetEnumerator();
                        this.CycleTargets();
                        if (target.Map.IsUndiscovered(target.Global))// .GetCell(target.Global).IsExposed)
                            this.LabelName.TextFunc = () => "Unknown block";
                    }
                    break;

                case TargetType.Null:
                    this.Hide();
                    this.LabelName.TextFunc = () => "<none>";
                    if (this.WindowInfo != null)
                        this.WindowInfo.Hide();
                    this.WindowInfo = null;
                    this.SelectedSource = TargetArgs.Null;
                    this.Selectable = null;
                    //this.MultipleSelected = new TargetArgs[] { };
                    this.MultipleSelected.Clear();
                    return;

                default:
                    break;
            }
            this.SelectedSource = target;
            this.Show();
            //if (target.Type != TargetType.Null)
            //    target.Network.EventOccured(Message.Types.SelectedChanged, target);
            if (target.Type == TargetType.Entity)
                target.Object.Net.EventOccured(Message.Types.SelectedChanged, target);
            this.WindowManager.OnSelectedTargetChanged(target);
            this.Validate(true);
        }
        public override bool Show()
        {
            //this.Location = Instance.BottomCenterScreen;
            this.AnchorToBottomCenter();
            Instance.BoxButtons.AlignLeftToRight();
            this.BoxButtons.Show();
            return base.Show();
        }
        public override bool Hide()
        {
            this.BoxButtons.Hide();
            return base.Hide();
        }
        private void Clear()
        {
            foreach (var a in this.ActionsAdded)
                a.Value.Clear();
            this.ActionsAdded.Clear();
            this.BoxTabs.ClearControls();
            this.BoxButtons.ClearControls();
            this.BoxInfo.ClearControls();
            this.PanelInfo.ClearControls();
            this.PopulateBoxIcons();
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxInfo,
                this.BoxIcons);
        }

        private void CycleTargets()
        {
            if (this.SelectedStack == null)
                return;
            this.SelectedStack.MoveNext();
            var first = this.SelectedStack.Current;
            this.SetName(first.GetName());
            this.Clear();

            first.GetSelectionInfo(this);
            first.GetQuickButtons(this);
            this.InitInfoTabs(first.GetInfoTabs());
            Net.Client.Instance.Map.Town.Select(first, this);
            this.Selectable = first;
        }
        void InitInfoTabs(IEnumerable<(string name, Action action)> tabs)
        {
            foreach (var (name, action) in tabs)
                this.AddTabAction(name, action, Color.Orange);
        }
        void InitInfoTabs(IEnumerable<Button> tabs)
        {
            foreach (var button in tabs)
                this.AddTabAction(button);
        }
        internal static bool IsSelected(ISelectable item)
        {
            if (Instance.SelectedStack == null)
                return false;
            return Instance.SelectedStack.Current == item;
        }

        readonly Dictionary<Action<List<TargetArgs>>, List<TargetArgs>> ActionsAdded = new();

        private void CreateButtons(IEnumerable<TargetArgs> targets)
        {
            this.BoxButtons.ClearControls();
            this.ActionsAdded.Clear();
            foreach (var tar in targets)
                tar.GetQuickButtons(this);
            Net.Client.Instance.Map.Town.Select(null, this);

        }
        void AddTabAction(Button button)
        {
            button.BackgroundColor = UIManager.TintPrimary * .5f;
            this.BoxTabs.AddControlsLineWrap(new[] { button }, this.PanelInfo.Width);
        }
        void AddTabAction(string label, Action action, Color col)
        {
            this.BoxTabs.AddControlsLineWrap(new[] { new Button(label) { LeftClickAction = action, BackgroundColor = col * .5f } }, this.PanelInfo.Width);
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
            Instance.AddButtons(new IconButton[] { button });
        }
        private void MultipleSelectedAction(Action<List<TargetArgs>> action)
        {
            action(this.ActionsAdded[action]);
        }

        public override void Update()
        {
            base.Update();

            /// move this to ongameevent?
            if (this.SelectedSource is not null && this.SelectedSource.Type == TargetType.Entity && this.SelectedSource.Object.IsDisposed)
                this.SelectInternal(TargetArgs.Null);

            if (this.Selectable is null)
            {
                if (!this.MultipleSelected.Any())
                    if (this.IsOpen)
                        this.Hide();
                return;
            }

            /// do i really need this? i handle the blockschanged message anyway, and this causes problems for selecting undiscovered air blocks 
            //if (!this.Selectable.Exists) 
            //    this.SelectInternal(TargetArgs.Null);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.BlocksChanged:
                    var map = e.Parameters[0] as MapBase;
                    var globals = e.Parameters[1] as IEnumerable<IntVec3>;// IntVec3[];
                    //var targets = globals.Select(g => new TargetArgs(map, g));
                    if (globals.Any(t => IsSelected(t)))
                        ClearTargets();
                    break;

                case Message.Types.EntityDespawned:
                    // TODO: deselect entity on despawn?
                    var entity = e.Parameters[0] as GameObject;
                    if (this.MultipleSelected.FirstOrDefault(t => t.Object == entity) is TargetArgs t)
                        this.MultipleSelected.Remove(t);
                    break;

                default:
                    base.OnGameEvent(e);
                    break;
            }
        }
        bool IsSelected(MapBase map, Vector3 global)
        {
            return this.MultipleSelected.Any(t => (t.Type == TargetType.Position && t.Map == map && t.Global == global) || (this.SelectedSource?.Global == global));
        }
        public override void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Any())
            {
                //foreach (var obj in this.MultipleSelected)
                //    if (obj.Type == TargetType.Position)
                //    {
                //        camera.DrawBlockMouseover(sb, obj.Map, obj.Global, Color.Yellow);
                //        //ToolManager.DrawBlockMouseover(sb, obj.Map, camera, obj);
                //        var map = obj.Map;
                //        var global = obj.Global;
                //        //map.GetBlock(global).DrawSelected(sb, camera, map, global);
                //    }
                var first = this.MultipleSelected.First();
                var map = first.Map;
                if (first.Type == TargetType.Position)
                    Renderer.DrawBlocks(map, camera, this.MultipleSelected.Select(t => (IntVec3)t.Global));
                if (SingleSelectedCell.HasValue)
                {
                    var singleCell = SingleSelectedCell.Value;
                    map.GetBlock(singleCell).DrawSelected(sb, camera, map, singleCell);
                    //map.GetBlock(singleCell).DrawInteractionSpots(sb, camera, map, singleCell);
                }
            }
            else if (this.SelectedSource != null) // this block never executes aymore because even single selections are contained in the multipleselected collection
            {
                if (this.SelectedSource.Type == TargetType.Position)
                {
                    //camera.DrawBlockMouseover(sb, this.SelectedSource.Map, this.SelectedSource.Global, Color.Yellow);
                    camera.DrawCellHighlights(Block.BlockHighlight, new IntVec3[] { this.SelectedSource.Global }, Color.Yellow);
                    var map = this.SelectedSource.Map;
                    var global = this.SelectedSource.Global;
                    map.GetBlock(global).DrawSelected(sb, camera, map, global);
                }
            }
        }
        public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Any())
                foreach (var obj in this.MultipleSelected)
                    if (obj.Type == TargetType.Entity)
                        obj.Object.DrawBorder(sb, camera);
                    else if (this.SelectedSource != null)
                        if (this.SelectedSource.Type == TargetType.Entity)
                            this.SelectedSource.Object.DrawBorder(sb, camera);
        }

        public static bool IsSelected(TargetArgs tar)
        {
            return Instance.MultipleSelected.Any(t => t.IsEqual(tar)) || Instance.SelectedSource.IsEqual(tar);
        }
        public static bool IsSelected(IntVec3 tar)
        {
            return 
                Instance.MultipleSelected.Any(t => t.Type == TargetType.Position && (IntVec3)t.Global == tar)  || 
                Instance.SelectedSource.Type == TargetType.Position && (IntVec3)Instance.SelectedSource.Global == tar;
        }
        internal static bool ClearTargets()
        {
            if (!Instance.MultipleSelected.Any() && Instance.SelectedSource.Type == TargetType.Null)
                return false;
            Instance.SelectInternal(TargetArgs.Null);
            return true;
        }

        //static void Slice()
        //{
        //    ScreenManager.CurrentScreen.Camera.SliceOn((int)Instance.SelectedSource.Global.Z);
        //    //var current = ScreenManager.CurrentScreen.Camera.DrawLevel;
        //    //if (next != current)
        //    //{
        //    //    Instance.PreviousDrawLevel = current;
        //    //    ScreenManager.CurrentScreen.Camera.DrawLevel = next;
        //    //}
        //    //else if (Instance.PreviousDrawLevel != -1)
        //    //    ScreenManager.CurrentScreen.Camera.DrawLevel = Instance.PreviousDrawLevel;
        //}
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
            this.BoxInfo.AlignVertically();
        }
        public static void RemoveInfo(Control ctrl)
        {
            Instance.BoxInfo.RemoveControls(ctrl);
        }
        public static void AddInfoNew(Control ctrl)
        {
            Instance.BoxInfo.AddControls(ctrl);
        }
        public static void RemoveButton(IconButton button)
        {
            Instance.BoxButtons.RemoveControls(button);
        }

        internal void AddButtons(params IconButton[] buttons)
        {
            this.BoxButtons.AddControls(buttons);
        }
        internal void AddButton(IconButton button, Action<List<TargetArgs>> action, GameObject obj, bool singleTargetOnly = false)
        {
            this.AddButton(button, action, new TargetArgs(obj), singleTargetOnly);
        }
        internal void AddButton(IconButton button, Action<TargetArgs> action, TargetArgs target)
        {
            this.AddButton(button, targets => action(targets.First()), target, true);
        }
        internal void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs obj, bool singleTargetOnly = false)
        {
            if (singleTargetOnly && this.MultipleSelected.Count > 1)
                return;

            if (this.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                existing.Add(obj);
                return;
            }
            else
                this.ActionsAdded.Add(action, new List<TargetArgs>() { obj });
            button.LeftClickAction = () => this.MultipleSelectedAction(action);
            this.BoxButtons.AddControls(button);
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<GameObject> targets)
        {
            AddButton(button, action, targets.Select(t => new TargetArgs(t)));
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<IntVec3> cells)
        {
            AddButton(button, action, cells.Select(t => new TargetArgs(t)));
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<TargetArgs> targets)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                Instance.ActionsAdded.Remove(action);
            }
            Instance.ActionsAdded.Add(action, targets.ToList());
            if (!Instance.BoxButtons.Controls.Contains(button))
                Instance.BoxButtons.AddControls(button);
            button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs target)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                existing.Add(target);
            }
            else
            {
                Instance.ActionsAdded.Add(action, new List<TargetArgs>() { target });
                Instance.BoxButtons.AddControls(button);
                button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
            }
        }

        internal static IEnumerable<TargetArgs> Selected => Instance.MultipleSelected;

        internal static IEnumerable<GameObject> GetSelectedEntities()
        {
            return Selected
                .Where(tar => tar.Type == TargetType.Entity)
                .Select(t => t.Object);
        }
        internal static IEnumerable<IntVec3> GetSelectedCells()
        {
            return Selected
                .Where(tar => tar.Type == TargetType.Position)
                .Select(t => (IntVec3)t.Global);
        }
        internal static IEnumerable<IntVec3> SelectedCells => GetSelectedCells();
        internal static IEnumerable<GameObject> SelectedEntities => GetSelectedEntities();

        internal static TargetArgs SingleSelected => Instance.MultipleSelected.Count == 1 ? Instance.MultipleSelected.Single() : null;
        internal static Entity SingleSelectedEntity => SingleSelected?.Object as Entity;
        internal static IntVec3? SingleSelectedCell => (SingleSelected is TargetArgs target && target.Type == TargetType.Position) ? target.Global : null;
        internal static BlockEntity SelectedBlockEntity => (SingleSelected is TargetArgs target && target.Type == TargetType.Position) ? target.BlockEntity : null;
    }

    [EnsureStaticCtorCall]
    public sealed class SelectionManagerNew
    {
        readonly GroupBox BoxTabs, BoxButtons, BoxIcons, BoxInfo;
        public Panel PanelInfo;
        public Label LabelName;
        readonly IconButton IconInfo, IconCenter, IconDetails;
        readonly IconButton IconCycle;
        readonly IconButton IconIssues;
        static readonly BlockRendererNew Renderer = new(Block.BlockHighlight);
        static SelectionManagerNew()
        {
        }
        static readonly IconButton IconSlice = new(Icon.ArrowDown)
        {
            BackgroundTexture = UIManager.Icon16Background,
            LeftClickAction = ToolManagement.Slice,
            HoverText = "Slice z-level"
        };
        public static SelectionManagerNew Instance = new();

        public TargetArgs SelectedSource = TargetArgs.Null;
        ISelectable Selectable;
        Window WindowInfo;
        IEnumerator<ISelectable> SelectedStack;
        public List<TargetArgs> MultipleSelected = new(); // TODO: make this a list of iselectables

        SelectionManagerNew()
        {
            //this.PanelInfo = new Panel(new Rectangle(0, 0, 300, 100));
            this.PanelInfo = Panel.FromClientSize(302, Label.DefaultHeight * 6);// 100); (302 = fit 3 x 100px widt bars, width 1 px spacing between them
            this.BoxTabs = new GroupBox()
            {
                AutoSize = false,
                //Size = new Rectangle(0, 0, 300, Button.DefaultHeight)
                Size = new Rectangle(0, 0, this.PanelInfo.Width, Button.DefaultHeight)
            };
            this.BoxTabs.AnchorTo(() => this.PanelInfo.ScreenLocation, Vector2.UnitY);

            //this.PanelInfo.Location = this.BoxTabs.BottomLeft;
            this.PanelInfo.AnchorToBottomCenter();
            this.LabelName = new Label() { TextFunc = () => "<none>" };
            Lazy<SelectionDetailsGui> detailsGui = new Lazy<SelectionDetailsGui>(() => new SelectionDetailsGui());
            this.IconDetails = new IconButton("^")
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = () =>
                {
                    //detailsGui.Value.Refresh(Instance.SelectedSource ?? Instance.SelectedStack.Current);
                    detailsGui.Value.Refresh(Instance.SelectedSource ?? Instance.SelectedStack.Current).GetOrCreateWindow("Details").Toggle();
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
                LeftClickAction = this.CycleTargets,
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

            this.BoxButtons = new GroupBox();
            this.BoxButtons.BackgroundColorFunc = () => Color.Black * .5f;
            this.BoxButtons.LocationFunc = () => this.PanelInfo.BottomRight;
            this.BoxButtons.Anchor = new Vector2(0, 1);
            this.BoxButtons.ControlsChangedAction = () => this.BoxButtons.AlignLeftToRight();


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

            if (this.SelectedStack != null)
                this.BoxIcons.AddControls(this.IconCycle);

            this.RepositionsBoxIcons();
        }

        private void CenterCamera()
        {
            if (this.SelectedSource != null)
                if (this.SelectedSource.Type != TargetType.Null)
                    ScreenManager.CurrentScreen.Camera.CenterOn(this.SelectedSource.Global);// .CenterOn(this.SelectedSource.Global);
        }
        public void SetName(string text)
        {
            this.LabelName.TextFunc = () => text;
        }
        private static void ToggleInfo()
        {
            if (Instance.SelectedSource.Object is Inspectable obj)
                Inspector.Refresh(obj);
            else
            {
                if (Instance.SelectedStack.Current is Inspectable insp)
                    Inspector.Refresh(insp);
                else
                    Inspector.Refresh(Instance.SelectedSource);
            }
            Inspector.Show();
        }

        public static void Select(TargetArgs target)
        {
            Instance.SelectInternal(target);
        }
        public static void Select(MapBase map, BoundingBox box)
        {
            Select(map.GetObjects(box).Select(s => new TargetArgs(s)));
        }
        public static void Select(IEnumerable<GameObject> entities)
        {
            Select(entities.Select(e => new TargetArgs(e)));
        }
        public static void Select(IEnumerable<TargetArgs> targets)
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
            Instance.SelectInternal(cells.Select(c => new TargetArgs(map, c)));
        }

        internal static void OnCameraRotated(Camera camera)
        {
            Renderer.Invalidate();
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
        internal static void AddToSelection(IEnumerable<TargetArgs> targets)
        {
            var list = Instance.MultipleSelected.Where(t => !targets.Any(t2 => t2.IsEqual(t))).Concat(targets).ToList();
            Instance.SelectInternal(list);
        }
        internal static void AddToSelection(TargetArgs target)
        {
            var existing = Instance.MultipleSelected.FirstOrDefault(t => t.IsEqual(target));
            if (existing != null)
                Instance.SelectInternal(Instance.MultipleSelected.Except(new TargetArgs[] { existing }));
            else
                Instance.SelectInternal(Instance.MultipleSelected.Concat(new TargetArgs[] { target }).ToList());
        }
        private IEnumerable<TargetArgs> FilterActors(IEnumerable<TargetArgs> targets)
        {
            if (targets.Any(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>()))
                return targets.Where(i => i.Type == TargetType.Entity && i.Object.HasComponent<NpcComponent>());
            return targets;
        }

        private void SelectInternal(IEnumerable<TargetArgs> targets)
        {
            this.SelectInternal(TargetArgs.Null);
            this.MultipleSelected = this.FilterActors(targets).Where(t => t.Exists).ToList();
            if (this.MultipleSelected.Count == 0)
                return;
            if (this.MultipleSelected.Count == 1)
            {
                this.SelectInternal(targets.First());
                return;
            }

            this.LabelName.TextFunc = () => $"Multiple x{this.MultipleSelected.Count}";

            this.CreateButtons(targets);
            this.PanelInfo.RemoveControls(this.BoxIcons);
            this.Show();
        }
        private void SelectInternal(TargetArgs target)
        {
            Renderer.Invalidate();

            if (this.SelectedSource.IsEqual(target))
            {
                this.CycleTargets();
                return;
            }
            this.SelectedSource = target;
            this.SelectedStack = null;
            this.WindowInfo = null;
            this.Clear();
            switch (target.Type)
            {
                case TargetType.Entity:
                    var entity = target.Object;
                    this.LabelName.TextFunc = () => entity.GetName();
                    //this.MultipleSelected = new TargetArgs[] { target };
                    this.MultipleSelected.Clear();
                    this.MultipleSelected.Add(target);
                    entity.GetSelectionInfo(this);
                    entity.GetQuickButtons(this);
                    this.InitInfoTabs(entity.GetTabs());
                    entity.Town.Select(target, this);
                    this.InitInfoTabs(entity.Town.GetTabs(target));
                    break;

                case TargetType.Position:
                    //this.MultipleSelected = new TargetArgs[1] { target };
                    this.MultipleSelected.Clear();
                    this.MultipleSelected.Add(target);
                    var selectables = target.Map.Town.QuerySelectables(target);
                    if (selectables.Any())
                    {
                        this.SelectedStack = selectables.GetEnumerator();
                        this.CycleTargets();
                        if (target.Map.IsUndiscovered(target.Global))// .GetCell(target.Global).IsExposed)
                            this.LabelName.TextFunc = () => "Unknown block";
                    }
                    break;

                case TargetType.Null:
                    //this.Hide();
                    this.PanelInfo.Hide();
                    this.BoxButtons.Hide();
                    this.LabelName.TextFunc = () => "<none>";
                    if (this.WindowInfo != null)
                        this.WindowInfo.Hide();
                    this.WindowInfo = null;
                    this.SelectedSource = TargetArgs.Null;
                    this.Selectable = null;
                    //this.MultipleSelected = new TargetArgs[] { };
                    this.MultipleSelected.Clear();
                    return;

                default:
                    break;
            }
            this.SelectedSource = target;
            this.Show();

            //if (target.Type != TargetType.Null)
            //    target.Network.EventOccured(Message.Types.SelectedChanged, target);
            if (target.Type == TargetType.Entity)
                target.Object.Net.EventOccured(Message.Types.SelectedChanged, target);
            this.PanelInfo.WindowManager.OnSelectedTargetChanged(target);
            this.PanelInfo.Validate(true);
        }
        void Show()
        {
            this.BoxTabs.Show();
            this.PanelInfo.Show();
            this.BoxButtons.Show();
        }
        //public override bool Show()
        //{
        //    //this.Location = Instance.BottomCenterScreen;
        //    this.AnchorToBottomCenter();
        //    Instance.BoxButtons.AlignLeftToRight();
        //    this.BoxButtons.Show();
        //    return base.Show();
        //}
        void Hide()
        {
            this.BoxTabs.Hide();
            this.PanelInfo.Hide();
            this.BoxButtons.Hide();
        }
        private void Clear()
        {
            foreach (var a in this.ActionsAdded)
                a.Value.Clear();
            this.ActionsAdded.Clear();
            this.BoxTabs.ClearControls();
            this.BoxButtons.ClearControls();
            this.BoxInfo.ClearControls();
            this.PanelInfo.ClearControls();
            this.PopulateBoxIcons();
            this.PanelInfo.AddControls(
                this.LabelName,
                this.BoxInfo,
                this.BoxIcons);
        }

        private void CycleTargets()
        {
            if (this.SelectedStack == null)
                return;
            this.SelectedStack.MoveNext();
            var first = this.SelectedStack.Current;
            this.SetName(first.GetName());
            this.Clear();

            first.GetSelectionInfo(this);
            first.GetQuickButtons(this);
            this.InitInfoTabs(first.GetInfoTabs());
            Net.Client.Instance.Map.Town.Select(first, this);
            this.Selectable = first;
        }
        void InitInfoTabs(IEnumerable<(string name, Action action)> tabs)
        {
            foreach (var (name, action) in tabs)
                this.AddTabAction(name, action, Color.Orange);
        }
        void InitInfoTabs(IEnumerable<Button> tabs)
        {
            foreach (var button in tabs)
                this.AddTabAction(button);
        }
        internal static bool IsSelected(ISelectable item)
        {
            if (Instance.SelectedStack == null)
                return false;
            return Instance.SelectedStack.Current == item;
        }

        readonly Dictionary<Action<List<TargetArgs>>, List<TargetArgs>> ActionsAdded = new();

        private void CreateButtons(IEnumerable<TargetArgs> targets)
        {
            this.BoxButtons.ClearControls();
            this.ActionsAdded.Clear();
            foreach (var tar in targets)
                tar.GetQuickButtons(this);
            Net.Client.Instance.Map.Town.Select(null, this);

        }
        void AddTabAction(Button button)
        {
            button.BackgroundColor = UIManager.TintPrimary * .5f;
            this.BoxTabs.AddControlsLineWrap(new[] { button }, this.PanelInfo.Width);
        }
        void AddTabAction(string label, Action action, Color col)
        {
            this.BoxTabs.AddControlsLineWrap(new[] { new Button(label) { LeftClickAction = action, BackgroundColor = col * .5f } }, this.PanelInfo.Width);
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
            Instance.AddButtons(new IconButton[] { button });
        }
        private void MultipleSelectedAction(Action<List<TargetArgs>> action)
        {
            action(this.ActionsAdded[action]);
        }

        public void Update()
        {
            /// move this to ongameevent?
            if (this.SelectedSource is not null && this.SelectedSource.Type == TargetType.Entity && this.SelectedSource.Object.IsDisposed)
                this.SelectInternal(TargetArgs.Null);

            if (this.Selectable is null)
            {
                if (!this.MultipleSelected.Any())
                    if (this.PanelInfo.IsOpen)
                        this.Hide();
                return;
            }

            /// do i really need this? i handle the blockschanged message anyway, and this causes problems for selecting undiscovered air blocks 
            //if (!this.Selectable.Exists) 
            //    this.SelectInternal(TargetArgs.Null);
        }
        internal void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.BlocksChanged:
                    var map = e.Parameters[0] as MapBase;
                    var globals = e.Parameters[1] as IEnumerable<IntVec3>;// IntVec3[];
                    //var targets = globals.Select(g => new TargetArgs(map, g));
                    if (globals.Any(t => IsSelected(t)))
                        ClearTargets();
                    break;

                case Message.Types.EntityDespawned:
                    // TODO: deselect entity on despawn?
                    var entity = e.Parameters[0] as GameObject;
                    if (this.MultipleSelected.FirstOrDefault(t => t.Object == entity) is TargetArgs t)
                        this.MultipleSelected.Remove(t);
                    break;

                default:
                    break;
            }
        }
        bool IsSelected(MapBase map, Vector3 global)
        {
            return this.MultipleSelected.Any(t => (t.Type == TargetType.Position && t.Map == map && t.Global == global) || (this.SelectedSource?.Global == global));
        }
        public void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Any())
            {
                //foreach (var obj in this.MultipleSelected)
                //    if (obj.Type == TargetType.Position)
                //    {
                //        camera.DrawBlockMouseover(sb, obj.Map, obj.Global, Color.Yellow);
                //        //ToolManager.DrawBlockMouseover(sb, obj.Map, camera, obj);
                //        var map = obj.Map;
                //        var global = obj.Global;
                //        //map.GetBlock(global).DrawSelected(sb, camera, map, global);
                //    }
                var first = this.MultipleSelected.First();
                var map = first.Map;
                if (first.Type == TargetType.Position)
                    Renderer.DrawBlocks(map, camera, this.MultipleSelected.Select(t => (IntVec3)t.Global));
                if (SingleSelectedCell.HasValue)
                {
                    var singleCell = SingleSelectedCell.Value;
                    map.GetBlock(singleCell).DrawSelected(sb, camera, map, singleCell);
                    //map.GetBlock(singleCell).DrawInteractionSpots(sb, camera, map, singleCell);
                }
            }
            else if (this.SelectedSource != null) // this block never executes aymore because even single selections are contained in the multipleselected collection
            {
                if (this.SelectedSource.Type == TargetType.Position)
                {
                    //camera.DrawBlockMouseover(sb, this.SelectedSource.Map, this.SelectedSource.Global, Color.Yellow);
                    camera.DrawCellHighlights(Block.BlockHighlight, new IntVec3[] { this.SelectedSource.Global }, Color.Yellow);
                    var map = this.SelectedSource.Map;
                    var global = this.SelectedSource.Global;
                    map.GetBlock(global).DrawSelected(sb, camera, map, global);
                }
            }
        }
        public void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            if (this.MultipleSelected.Any())
                foreach (var obj in this.MultipleSelected)
                    if (obj.Type == TargetType.Entity)
                        obj.Object.DrawBorder(sb, camera);
                    else if (this.SelectedSource != null)
                        if (this.SelectedSource.Type == TargetType.Entity)
                            this.SelectedSource.Object.DrawBorder(sb, camera);
        }

        public static bool IsSelected(TargetArgs tar)
        {
            return Instance.MultipleSelected.Any(t => t.IsEqual(tar)) || Instance.SelectedSource.IsEqual(tar);
        }
        public static bool IsSelected(IntVec3 tar)
        {
            return
                Instance.MultipleSelected.Any(t => t.Type == TargetType.Position && (IntVec3)t.Global == tar) ||
                Instance.SelectedSource.Type == TargetType.Position && (IntVec3)Instance.SelectedSource.Global == tar;
        }
        internal static bool ClearTargets()
        {
            if (!Instance.MultipleSelected.Any() && Instance.SelectedSource.Type == TargetType.Null)
                return false;
            Instance.SelectInternal(TargetArgs.Null);
            return true;
        }

        //static void Slice()
        //{
        //    ScreenManager.CurrentScreen.Camera.SliceOn((int)Instance.SelectedSource.Global.Z);
        //    //var current = ScreenManager.CurrentScreen.Camera.DrawLevel;
        //    //if (next != current)
        //    //{
        //    //    Instance.PreviousDrawLevel = current;
        //    //    ScreenManager.CurrentScreen.Camera.DrawLevel = next;
        //    //}
        //    //else if (Instance.PreviousDrawLevel != -1)
        //    //    ScreenManager.CurrentScreen.Camera.DrawLevel = Instance.PreviousDrawLevel;
        //}
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
            this.BoxInfo.AlignVertically();
        }
        public static void RemoveInfo(Control ctrl)
        {
            Instance.BoxInfo.RemoveControls(ctrl);
        }
        public static void AddInfoNew(Control ctrl)
        {
            Instance.BoxInfo.AddControls(ctrl);
        }
        public static void RemoveButton(IconButton button)
        {
            Instance.BoxButtons.RemoveControls(button);
        }

        internal void AddButtons(params IconButton[] buttons)
        {
            this.BoxButtons.AddControls(buttons);
        }
        internal void AddButton(IconButton button, Action<List<TargetArgs>> action, GameObject obj, bool singleTargetOnly = false)
        {
            this.AddButton(button, action, new TargetArgs(obj), singleTargetOnly);
        }
        internal void AddButton(IconButton button, Action<TargetArgs> action, TargetArgs target)
        {
            this.AddButton(button, targets => action(targets.First()), target, true);
        }
        internal void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs obj, bool singleTargetOnly = false)
        {
            if (singleTargetOnly && this.MultipleSelected.Count > 1)
                return;

            if (this.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                existing.Add(obj);
                return;
            }
            else
                this.ActionsAdded.Add(action, new List<TargetArgs>() { obj });
            button.LeftClickAction = () => this.MultipleSelectedAction(action);
            this.BoxButtons.AddControls(button);
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<GameObject> targets)
        {
            AddButton(button, action, targets.Select(t => new TargetArgs(t)));
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<IntVec3> cells)
        {
            AddButton(button, action, cells.Select(t => new TargetArgs(t)));
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, IEnumerable<TargetArgs> targets)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                Instance.ActionsAdded.Remove(action);
            }
            Instance.ActionsAdded.Add(action, targets.ToList());
            if (!Instance.BoxButtons.Controls.Contains(button))
                Instance.BoxButtons.AddControls(button);
            button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
        }
        internal static void AddButton(IconButton button, Action<List<TargetArgs>> action, TargetArgs target)
        {
            if (Instance.ActionsAdded.TryGetValue(action, out List<TargetArgs> existing))
            {
                existing.Add(target);
            }
            else
            {
                Instance.ActionsAdded.Add(action, new List<TargetArgs>() { target });
                Instance.BoxButtons.AddControls(button);
                button.LeftClickAction = () => Instance.MultipleSelectedAction(action);
            }
        }

        internal static IEnumerable<TargetArgs> Selected => Instance.MultipleSelected;

        internal static IEnumerable<GameObject> GetSelectedEntities()
        {
            return Selected
                .Where(tar => tar.Type == TargetType.Entity)
                .Select(t => t.Object);
        }
        internal static IEnumerable<IntVec3> GetSelectedCells()
        {
            return Selected
                .Where(tar => tar.Type == TargetType.Position)
                .Select(t => (IntVec3)t.Global);
        }
        internal static IEnumerable<IntVec3> SelectedCells => GetSelectedCells();
        internal static IEnumerable<GameObject> SelectedEntities => GetSelectedEntities();

        internal static TargetArgs SingleSelected => Instance.MultipleSelected.Count == 1 ? Instance.MultipleSelected.Single() : null;
        internal static Entity SingleSelectedEntity => SingleSelected?.Object as Entity;
        internal static IntVec3? SingleSelectedCell => (SingleSelected is TargetArgs target && target.Type == TargetType.Position) ? target.Global : null;
        internal static BlockEntity SelectedBlockEntity => (SingleSelected is TargetArgs target && target.Type == TargetType.Position) ? target.BlockEntity : null;
    }
}
