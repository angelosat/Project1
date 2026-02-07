using Project1.Core.Towns.Crafting;

namespace Project1.Core.UI
{
    internal class OrderSettingsGui : GroupBox
    {
        readonly OrderSettings Settings;
        readonly Label LabelAmount;
        readonly ComboBoxNewNew<OrderSettings.CraftMode> ModeCBox;
        OrderSettings.CraftMode _modePredicted;
        int _amountPredicted;
        readonly Button btnDetails;
        readonly IconButton btnClose;

        public OrderSettingsGui(OrderSettings settings)
        {
            settings.Workstation.Map.Events.ListenTo<CraftOrderUpdatedEvent>(onCraftOrderModified);

            this.Settings = settings;
            this._modePredicted = settings.Mode;
            this._amountPredicted = settings.Amount;
           
            this.BackgroundColor = UIManager.DefaultListItemBackgroundColor;
            this.MouseThrough = false;

            var orderName = new Label(settings.Label);// { Location = btnUp.TopRight };
            this.ModeCBox = new ComboBoxNewNew<OrderSettings.CraftMode>(OrderSettings.AllModes, 100, c => c.ToString(), ChangeFinishMode, () => this._modePredicted)
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
            PacketsCrafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 1, 0, this.Settings.Mode);
        }
        void MoveUp()
        {
            PacketsCrafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, -1, 0, this.Settings.Mode);
        }
        void ChangeOrderPriority(bool p)
        {
        }
        void RemoveOrder()
        {
            PacketsCrafting.SendPlayerDeletedOrder(this.Settings.Workstation.Parent.Map, this.Settings);
        }
        void Minus()
        {
            this._amountPredicted--;
            PacketsCrafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, -1, this.Settings.Mode);
        }
        void Plus()
        {
            this._amountPredicted++;
            PacketsCrafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, 1, this.Settings.Mode);
        }
        void ChangeFinishMode(OrderSettings.CraftMode mode)
        {
            this._modePredicted = mode;
            PacketsCrafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, 0, mode);
        }
    }
}
