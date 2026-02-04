using Project1.Framework.Base;
using Project1.Framework.Screens;
using Project1.Framework.UI;
using Project1.Framework.WorldGen;
using Start_a_Town_.UI;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class WorkstationGuiNew : GroupBox, ISelectionBound
    {
        Panel PanelReactions;
        readonly ListBoxNoScroll<OrderSettings, OrderGuiContainer> ListOrdersNew;
        Table<(string label, Func<ZoneId> zoneIdGetter, WorkstationIOType iotype)> IOTable;
        BlockWorkstationComp Workstation;

        public ISelectable CurrentSelection { get; set; }

        public WorkstationGuiNew()
        {
            this.ListOrdersNew = new(s => new OrderGuiContainer(s, s => this.MoveUp(s), s => this.MoveDown(s)));

        }
        class OrderGuiContainer : GroupBox
        {
            public readonly ButtonIcon Up, Down;
            readonly Control ItemControl;
            public OrderGuiContainer(OrderSettings s, Action<OrderSettings> moveUp, Action<OrderSettings> modeDown)
            {
                this.Up = new ButtonIcon(Icon.ArrowUp, () => moveUp(s));
                this.Down = new ButtonIcon(Icon.ArrowDown, () => modeDown(s));
                this.ItemControl = s.GetListControlGui();
                this.AddControlsVertically(this.Up, this.Down)
                    .AddControlsTopRight(this.ItemControl);
            }
            public override void OnLayout(int availableWidth, int availableHeight)
            {
                this.Width = availableWidth;
                this.ItemControl.OnLayout(this.Width - this.Up.Width, this.Height);
            }
        }
        void Build(BlockWorkstationComp workstation)
        {
            this.Workstation = workstation;
            var btnAddOrder = new Button("Add Order", this.OnAddOrderClick);

            this.PanelReactions = new Panel() { AutoSize = true };
            this.PanelReactions.HideOnAnyClick();
            var manager = workstation.Parent.Map.Town.CraftingManagerNew;
            //var availableRecipes = CraftingSystem.GetCraftables(workstation.WorkstationType.Capabilities.First()); // HACK
            var availableRecipesNew = workstation.WorkstationType.Capabilities.SelectMany(cap => cap.Worker.GetAddOrderRequests(workstation));

            //var availableRefinementsControl = new ListBoxNoScroll<Def>(r => new Label(r.Label, () => this.PlaceOrderNew(r)));
            //availableRefinementsControl.AddItems(availableRecipes);
            var availableRefinementsControl = new ListBoxNoScroll<AddOrderRequest>(r => new Label(r.ProductDef?.Label ?? r.WorkstationCapability.Label, () => this.PlaceOrderNew(r)));
            availableRefinementsControl.AddItems(availableRecipesNew);
            var reactionsListContainer = availableRefinementsControl.ToScrollableBox(200, 400);
            this.PanelReactions.AddControls(reactionsListContainer);

            var scrollableContainer = new ScrollableBoxNewNewNew(300, 400, ScrollModes.Vertical);
            scrollableContainer.AddControls(this.ListOrdersNew);

            var panell = scrollableContainer.ToPanelLabeled("Orders");

            this.ListOrdersNew.Clear();
            this.ListOrdersNew.AddItems(workstation.Orders);

            UpdateArrows();

            var map = workstation.Parent.Map;
            var zonemanager = map.Town.ZoneManager;
            var stockpiles = zonemanager.GetZones<Stockpile>().Prepend(null);

            this.IOTable = new Table<(string label, Func<ZoneId> zoneIdGetter, WorkstationIOType iotype)>()
                .AddColumn("iotype", 100, item => new LabelNew(item.label), anchorX: 1)
                //.AddColumn("control", 200, item => new ComboBoxFinal<Stockpile>(stockpiles, 200, s => s?.Name ?? "-None-", s => select(item.iotype, s), () => (item.zoneIdGetter() is ZoneId id && id != ZoneId.Null ? zonemanager.GetZone<Stockpile>(id) : null)));
                .AddColumn("control", 200, item => new ComboBoxFinal<Stockpile>(stockpiles, 200, s => s?.Name ?? "-None-", s => select(item.iotype, s), () => zonemanager.GetZone<Stockpile>(item.zoneIdGetter())));

            this.IOTable.AddItems([
                ("Input", ()=>workstation.Input, WorkstationIOType.Input),
                ("Output", ()=>workstation.Output, WorkstationIOType.Output)
                ]);
            var linkedStockpiledPanel = this.IOTable.ToPanelLabeled("Linked Stockpiles");

            void select(WorkstationIOType iotype, Stockpile stockpile) =>
                Ingame.Instance.Events.Post(new PlayerSetWorkstationZoneEvent(workstation, iotype, stockpile));

            this.AddControls(
                panell,
                btnAddOrder, linkedStockpiledPanel
                );
            this.AlignTopToBottom();

            var mapEvents = this.Workstation.Parent.Map.Events;
            mapEvents.ListenTo<CellsInvalidatedEvent>(OnBlocksUpdated);
            mapEvents.ListenTo<CraftOrderAddedEvent>(OnCraftOrderAdded);
            mapEvents.ListenTo<CraftOrderRemovedEvent>(OnCraftOrderRemoved);
            mapEvents.ListenTo<CraftOrderReorderedEvent>(OnOrderReordered);

            mapEvents.ListenTo<WorkstationUpdatedEvent>(OnWorkstationUpdated);
        }

        private void OnWorkstationUpdated(WorkstationUpdatedEvent e)
        {
            if (e.Comp != this.Workstation)
                return;
            this.IOTable.Invalidate(true);
        }

        private void OnOrderReordered(CraftOrderReorderedEvent e)
        {
            if (e.Order.Workstation != this.Workstation)
                return;
            var newindex = e.Order.Workstation.Orders.IndexOf(e.Order);
            this.ListOrdersNew.Move(e.Order, newindex);
            UpdateArrows();

        }
        private void MoveDown(OrderSettings s)
        {
            // local ui prediction
            var newindex = s.Workstation.Orders.IndexOf(s) + 1;
            this.ListOrdersNew.Move(s, newindex);
            UpdateArrows();
            PacketsCrafting.SendPlayerModifiedOrder(s.Workstation.Parent.Map, s, 1, 0, s.Mode);
        }

        private void MoveUp(OrderSettings s)
        {
            // local ui prediction
            var newindex = s.Workstation.Orders.IndexOf(s) - 1;
            this.ListOrdersNew.Move(s, newindex);
            UpdateArrows();
            PacketsCrafting.SendPlayerModifiedOrder(s.Workstation.Parent.Map, s, -1, 0, s.Mode);
        }
        void UpdateArrows()
        {
            if (this.ListOrdersNew.Count == 0)
                return;
            this.ListOrdersNew[0].Up.RemoveFromParent();
            this.ListOrdersNew[this.ListOrdersNew.Count - 1].Down.RemoveFromParent();
            if (this.ListOrdersNew.Count > 1)
            {
                this.ListOrdersNew[0].Down.AddToParent();
                this.ListOrdersNew[this.ListOrdersNew.Count - 1].Up.AddToParent();
            }
            for (int i = 1; i < this.ListOrdersNew.Count - 1; i++)
            {
                this.ListOrdersNew[i].Up.AddToParent();
                this.ListOrdersNew[i].Down.AddToParent();
            }
        }
        private void OnCraftOrderRemoved(CraftOrderRemovedEvent e)
        {
            this.ListOrdersNew.RemoveItems(e.Order);
            UpdateArrows();

        }

        public override bool Show()
        {
            this.Workstation.Map.Events.ListenTo<CraftOrderAddedEvent>(OnCraftOrderAdded);
            return base.Show();
        }

        private void OnCraftOrderAdded(CraftOrderAddedEvent e)
        {
            if (this.Workstation != e.Comp)
                return;
            this.ListOrdersNew.AddItems(e.Order);
            OrderGuiContainer cntr = this.ListOrdersNew.GetControlFor(e.Order);
            UpdateArrows();

        }
        private void OnAddOrderClick()
        {
            this.PanelReactions.SnapToMouse();
            this.PanelReactions.Show();
        }
        private void PlaceOrder(MaterialRefinementDef r)
        {
            this.PanelReactions.Hide();
            PacketsCrafting.PlayerCreatedOrder(this.Workstation.Parent, r);
        }
        private void PlaceOrderNew(Def craftableProfile)
        {
            this.PanelReactions.Hide();
            Ingame.Instance.Events.Post(new PlayerIssuedCraftOrderEvent(this.Workstation, craftableProfile));
            //PacketsCrafting.PlayerCreatedOrder(this.Workstation.Parent, r);
        }
        private void PlaceOrderNew(AddOrderRequest orderRequest)
        {
            this.PanelReactions.Hide();
            Ingame.Instance.Events.Post(new PlayerIssuedCraftOrderEventNew(this.Workstation, orderRequest));
        }
        void OnBlocksUpdated(CellsInvalidatedEvent e)
        {
            if (e.Positions.Contains(this.Workstation.Global))
                this.GetWindow().Hide();
        }
        public void OnBind(ISelectable selectable)
        {
            if (!(selectable is TargetArgs target &&
                target.BlockEntityOld is BlockEntity block &&
                block.GetComp<BlockWorkstationComp>() is BlockWorkstationComp comp))
                throw new Exception();
            this.Build(comp);
        }
    }

    [Obsolete]
    class WorkstationGui : GroupBox
    {
        IntVec3 Global;
        readonly MapBase Map;
        readonly Panel PanelReactions;
        readonly ScrollableBoxNewNew ListOrders;
        public WorkstationGui()
        {
            
        }
        public WorkstationGui(MapBase map, IntVec3 global, BlockEntityCompWorkstationOld entity)
        {

            this.Global = global;
            this.Map = map;
            var panelOrders = new PanelTitled("Orders", 300, 500);
            var btnAddOrder = new Button("Add Order", this.AddOrder);

            this.PanelReactions = new Panel() { AutoSize = true };
            var allreactions = Def.GetDefs<Reaction>();
            var validreactions = allreactions.Where(r => r.ValidWorkshops.Any(t => entity.IsWorkstationType(t))).ToList();

            var reactionsList = new ListBoxNoScroll<Reaction>(r => new Label(r.Label, () => this.PlaceOrder(r)));
            reactionsList.AddItems(validreactions);
            var reactionsListContainer = reactionsList.ToScrollableBox(200, 400);
            this.PanelReactions.AddControls(reactionsListContainer);

            var w = panelOrders.Client.ClientSize.Width;
            var h = panelOrders.Client.ClientSize.Height;
            var list = entity.Orders.GetListObservableControl();
            this.ListOrders = new ScrollableBoxNewNew(w, h, ScrollModes.Vertical);
            this.ListOrders.AddControls(list);

            panelOrders.AddControls(this.ListOrders);

            this.AddControls(panelOrders, btnAddOrder);
            this.AlignTopToBottom();

            this.ListenTo<CellsInvalidatedEvent>(HandleBlocksChanged);
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.PanelReactions.HitTest() && this.PanelReactions.IsOpen)
                this.PanelReactions.Hide();
            base.HandleLButtonDown(e);
        }
        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.PanelReactions.Hide();
            base.HandleRButtonDown(e);
        }
        private void AddOrder()
        {
            this.PanelReactions.Location = UIManager.Mouse;
            this.PanelReactions.Show();
        }
        void PlaceOrder(Reaction r)
        {
            PacketsCrafting.Send(this.Map.Net, this.Global, r);
            this.PanelReactions.Hide();
        }
        void HandleBlocksChanged(CellsInvalidatedEvent e)
        {
            if (e.Positions.Contains(this.Global))
                this.GetWindow().Hide();
        }
    }
}
