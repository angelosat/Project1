using Project1.Core.WorldGen;
using System;
using System.Linq;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using Project1.Framework;
using Project1.Core.Crafting;

namespace Project1.Core.Legacy.Crafting.Gui
{

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

            var reactionsList = new ListBoxNoScroll<Reaction>(r => new Label(r.LabelReadable, () => this.PlaceOrder(r)));
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
