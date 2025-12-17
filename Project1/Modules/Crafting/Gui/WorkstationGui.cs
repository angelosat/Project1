using Start_a_Town_.UI;
using System;
using System.Collections;
using System.Linq;

namespace Start_a_Town_
{
    class WorkstationGuiNew : GroupBox, ISelectionBound
    {
        Panel PanelReactions;
        ListBoxNoScroll<OrderSettings> ListOrdersNew = new(s => s.GetListControlGui());
        BlockEntityCompWorkstation Workstation;
        public WorkstationGuiNew()
        {
            
        }
        void Build(BlockEntityCompWorkstation workstation)
        {
            this.Workstation = workstation;
            var panelOrders = new PanelTitled("Orders", 300, 400);
            var btnAddOrder = new Button("Add Order", this.OnAddOrderClick);

            this.PanelReactions = new Panel() { AutoSize = true };
            this.PanelReactions.HideOnAnyClick();
            var allreactions = Def.GetDefs<Reaction>();
            var manager = workstation.Parent.Map.Town.CraftingManagerNew;
            var availableProcesses = manager.GetProcessesFor(workstation.Type);
            var validreactions = allreactions;

            var reactionsList = new ListBoxNoScroll<MaterialMappingDef>(r => new Label(r.Label, () => this.PlaceOrder(r)));
            reactionsList.AddItems(availableProcesses);
            var reactionsListContainer = reactionsList.ToScrollableBox(200, 400);
            this.PanelReactions.AddControls(reactionsListContainer);

            var w = panelOrders.Client.ClientSize.Width;
            var h = panelOrders.Client.ClientSize.Height;

            var scrollableContainer = new ScrollableBoxNewNewNew(w, h, ScrollModes.Vertical);
            scrollableContainer.AddControls(this.ListOrdersNew);

            panelOrders.AddControls(this.ListOrdersNew);
            this.AddControls(panelOrders, btnAddOrder);
            this.AlignTopToBottom();

            var mapEvents = workstation.Parent.Map.Events;
            mapEvents.ListenTo<BlocksUpdatedEvent>(OnBlocksUpdated);
            mapEvents.ListenTo<CraftOrderCreatedEvent>(OnCraftOrderCreated);
        }

        public override bool Show()
        {
            this.Workstation.Map.Events.ListenTo<CraftOrderCreatedEvent>(OnCraftOrderCreated);
            return base.Show();
        }

        private void OnCraftOrderCreated(CraftOrderCreatedEvent e)
        {
            if (this.Workstation != e.Comp)
                return;
            this.ListOrdersNew.AddItems(e.Order);
            
        }
        private void OnAddOrderClick()
        {
            this.PanelReactions.SnapToMouse();
            this.PanelReactions.Show();
        }
        private void PlaceOrder(MaterialMappingDef r)
        {
            this.PanelReactions.Hide();
            PacketOrderAdd.PlayerCreatedOrder(this.Workstation.Parent, r);
        }
        void OnBlocksUpdated(BlocksUpdatedEvent e)
        {
            if (e.Positions.Contains(this.Workstation.Global))
                this.GetWindow().Hide();
        }
        public void Bind(ISelectable selectable)
        {
            if (selectable is TargetArgs target &&
                target.BlockEntity is BlockEntity block &&
                block.GetComp<BlockEntityCompWorkstation>() is BlockEntityCompWorkstation comp)
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
            PacketOrderAdd.Send(this.Map.Net, this.Global, r);
            this.PanelReactions.Hide();
        }
        void HandleBlocksChanged(BlocksUpdatedEvent e)
        {
            if (e.Positions.Contains(this.Global))
                this.GetWindow().Hide();
        }
    }
}
