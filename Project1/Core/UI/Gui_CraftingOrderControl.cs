using Project1.Core.Crafting;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    internal class Gui_CraftingOrderControl : GroupBox
    {
        readonly CraftingOrder Settings;
        readonly Label LabelAmount;
        readonly ComboBoxNewNew<CraftingOrder.CraftMode> ModeCBox;
        CraftingOrder.CraftMode _modePredicted;
        int _amountPredicted;
        readonly Button btnDetails;
        readonly IconButton btnClose;

        public Gui_CraftingOrderControl(CraftingOrder settings)
        {
            settings.Workstation.Map.Events.ListenTo<CraftOrderUpdatedEvent>(onCraftOrderModified);

            this.Settings = settings;
            this._modePredicted = settings.Mode;
            this._amountPredicted = settings.Amount;
           
            this.BackgroundColor = UIManager.DefaultListItemBackgroundColor;
            this.MouseThrough = false;

            var orderName = new Label(settings.LabelReadable);// { Location = btnUp.TopRight };
            this.ModeCBox = new ComboBoxNewNew<CraftingOrder.CraftMode>(CraftingOrder.AllModes, 100, c => c.ToString(), ChangeFinishMode, () => this._modePredicted)
            {
                AnchorNew = Anchors.Bottom | Anchors.Left
            };

            this.AddControls(orderName
                ,
                this.ModeCBox
                );
            var width = 0;// 290;
            this.btnClose = new IconButton(Icon.X)
            {
                BackgroundTexture = UIManager.Icon16Background,
                LeftClickAction = RemoveOrder
            };
            btnClose.ShowOnParentFocus(true);
            btnClose.AnchorNew = Anchors.Right | Anchors.Top;
            this.AddControls(btnClose);

            var btnMinus = new Button("-", Minus, Button.DefaultHeight) { Location = this.ModeCBox.TopRight };
            var btnPlus = new Button("+", Plus, Button.DefaultHeight) { Location = btnMinus.TopRight };
            this.LabelAmount = new Label(()=>this._amountPredicted.ToString()) { Location = btnPlus.TopRight };

            this.AddControls(btnMinus, btnPlus, this.LabelAmount);

            this.btnDetails = new Button("Details", ToggleDetails);
            this.btnDetails.AnchorNew = Anchors.Bottom | Anchors.Right;
            this.AddControls(btnDetails);

        }
        void ToggleDetails()
        {
            var win = new OrderSettingsGuiDetails(this.Settings).ToWindow("Filters");
            win.ToggleSmart();
        }
        public override void OnLayout(int availableWidth, int availableHeight)
        {
            this.Width = availableWidth;
            this.Height = availableHeight;
            base.OnLayout(availableWidth, availableHeight);
        }

        private void onCraftOrderModified(CraftOrderUpdatedEvent e)
        {
            if (this.Settings == e.Order)
            {
                this._modePredicted = e.Order.Mode;
                this._amountPredicted = e.Order.Amount;
            }
        }

        void MoveDown()
        {
            Packets_Crafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 1, 0, this.Settings.Mode);
        }
        void MoveUp()
        {
            Packets_Crafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, -1, 0, this.Settings.Mode);
        }
        void ChangeOrderPriority(bool p)
        {
        }
        void RemoveOrder()
        {
            Packets_Crafting.SendPlayerDeletedOrder(this.Settings.Workstation.Parent.Map, this.Settings);
        }
        void Minus()
        {
            this._amountPredicted--;
            Packets_Crafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, -1, this.Settings.Mode);
        }
        void Plus()
        {
            this._amountPredicted++;
            Packets_Crafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, 1, this.Settings.Mode);
        }
        void ChangeFinishMode(CraftingOrder.CraftMode mode)
        {
            this._modePredicted = mode;
            Packets_Crafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, 0, mode);
        }
    }
}
