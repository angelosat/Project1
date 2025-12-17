using Start_a_Town_.UI;
using System;
using System.Linq;

namespace Start_a_Town_
{
    class WorkstationGuiNew : GroupBox, ISelectionBound
    {
        MapBase Map;
        Panel PanelReactions;
        ScrollableBoxNewNew ListOrders;
        BlockEntityCompWorkstation Workstation;
        public WorkstationGuiNew()
        {
            
        }
        void Build(BlockEntityCompWorkstation workstation)
        {
            this.Workstation = workstation;
            var panelOrders = new PanelTitled("Orders", 300, 400);
            var btnAddOrder = new Button("Add Order", this.AddOrder);

            this.PanelReactions = new Panel() { AutoSize = true };
            var allreactions = Def.GetDefs<Reaction>();
            //var validreactions = allreactions.Where(r => r.ValidWorkshops.Any(t => this.Workstation.IsWorkstationType(t))).ToList();
            var validreactions = allreactions;//.Where(r => r.ValidWorkshops.Any(t => this.Workstation.IsWorkstationType(t))).ToList();

            var reactionsList = new ListBoxNoScroll<Reaction>(r => new Label(r.Label, () => this.PlaceOrder(r)));
            reactionsList.AddItems(validreactions);
            var reactionsListContainer = reactionsList.ToScrollableBox(200, 400);
            this.PanelReactions.AddControls(reactionsListContainer);

            var w = panelOrders.Client.ClientSize.Width;
            var h = panelOrders.Client.ClientSize.Height;
            //var list = workstation.Orders.GetListObservableControl();
            this.ListOrders = new ScrollableBoxNewNew(w, h, ScrollModes.Vertical);
            //this.ListOrders.AddControls(list);

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
            PacketOrderAdd.Send(this.Map.Net, this.Workstation.Global, r);
            this.PanelReactions.Hide();
        }
        void HandleBlocksChanged(BlocksUpdatedEvent e)
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
