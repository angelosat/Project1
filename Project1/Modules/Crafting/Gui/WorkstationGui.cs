using Start_a_Town_.UI;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class WorkstationGuiNew : GroupBox, ISelectionBound
    {
        Panel PanelReactions;
        //ListBoxNoScroll<OrderSettings> ListOrdersNew = new(s => s.GetListControlGui());
        //ListBoxNoScroll<OrderSettings> ListOrdersNew;
        ListBoxNoScroll<OrderSettings, OrderContainer> ListOrdersNew;

        

        BlockWorkstationComp Workstation;

        public WorkstationGuiNew()
        {
            this.ListOrdersNew = new(s => new OrderContainer(s, s => this.MoveUp(s), s => this.MoveDown(s)));
        }
        class OrderContainer : GroupBox
        {
            public readonly ButtonIcon Up, Down;
            Control ItemControl;
            public OrderContainer(OrderSettings s, Action<OrderSettings> moveUp, Action<OrderSettings> modeDown)
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
            //var panelOrders = new PanelTitled("Orders", 300, 400);
            //var panelOrders = new Panel(new Rectangle(0,0, 300, 400));
            var btnAddOrder = new Button("Add Order", this.OnAddOrderClick);

            this.PanelReactions = new Panel() { AutoSize = true };
            this.PanelReactions.HideOnAnyClick();
            //var allreactions = Def.GetDefs<Reaction>();
            var manager = workstation.Parent.Map.Town.CraftingManagerNew;
            var availableRefinements = manager.GetRefinementsBy(workstation.WorkstationType);
            //var validreactions = allreactions;

            var availableRefinementsControl = new ListBoxNoScroll<MaterialRefinementDef>(r => new Label(r.Label, () => this.PlaceOrder(r)));
            availableRefinementsControl.AddItems(availableRefinements);
            var reactionsListContainer = availableRefinementsControl.ToScrollableBox(200, 400);
            this.PanelReactions.AddControls(reactionsListContainer);

            //var w = panelOrders.Client.ClientSize.Width;
            //var h = panelOrders.Client.ClientSize.Height;
            var scrollableContainer = new ScrollableBoxNewNewNew(300, 400, ScrollModes.Vertical);
            scrollableContainer.AddControls(this.ListOrdersNew);

            //panelOrders.AddControls(this.ListOrdersNew);
            //panelOrders.AddControls(scrollableContainer);

            var panell = scrollableContainer.ToPanelLabeled("Orders");

            this.ListOrdersNew.Clear();
            this.ListOrdersNew.AddItems(workstation.Orders);

            UpdateArrows();

            this.AddControls(
                //panelOrders,
                panell,
                btnAddOrder);
            this.AlignTopToBottom();

            var mapEvents = this.Workstation.Parent.Map.Events;
            mapEvents.ListenTo<BlocksUpdatedEvent>(OnBlocksUpdated);
            mapEvents.ListenTo<CraftOrderAddedEvent>(OnCraftOrderAdded);
            mapEvents.ListenTo<CraftOrderRemovedEvent>(OnCraftOrderRemoved);
            mapEvents.ListenTo<CraftOrderReorderedEvent>(OnOrderReordered);
        }
        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{

        //    base.Draw(sb, viewport);
        //}

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
            PacketPlayerCraftOrders.PlayerModifiedOrder(s.Workstation.Parent.Map, s, 1, 0, s.Mode);
        }

        private void MoveUp(OrderSettings s)
        {
            // local ui prediction
            var newindex = s.Workstation.Orders.IndexOf(s) - 1;
            this.ListOrdersNew.Move(s, newindex);
            UpdateArrows();
            PacketPlayerCraftOrders.PlayerModifiedOrder(s.Workstation.Parent.Map, s, -1, 0, s.Mode);
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
            OrderContainer cntr = this.ListOrdersNew.GetControlFor(e.Order);
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
            PacketPlayerCraftOrders.PlayerCreatedOrder(this.Workstation.Parent, r);
        }
        void OnBlocksUpdated(BlocksUpdatedEvent e)
        {
            if (e.Positions.Contains(this.Workstation.Global))
                this.GetWindow().Hide();
        }
        public void Bind(ISelectable selectable)
        {
            if (!(selectable is TargetArgs target &&
                target.BlockEntityOld is BlockEntity block &&
                block.GetComp<BlockWorkstationComp>() is BlockWorkstationComp comp))
                throw new Exception();
            this.Build(comp);
        }
    }
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

            this.ListenTo<BlocksUpdatedEvent>(HandleBlocksChanged);
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
            PacketPlayerCraftOrders.Send(this.Map.Net, this.Global, r);
            this.PanelReactions.Hide();
        }
        void HandleBlocksChanged(BlocksUpdatedEvent e)
        {
            if (e.Positions.Contains(this.Global))
                this.GetWindow().Hide();
        }
    }
}
