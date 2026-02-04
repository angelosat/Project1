using Project1.Framework.UI;
using Start_a_Town_.UI;

namespace Start_a_Town_
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
            };// { Location = orderName.BottomLeft };

            this.AddControls(orderName
                ,
                this.ModeCBox
                );
            var width = 0;// 290;
            this.btnClose = new IconButton(Icon.X)
            {
                //LocationFunc = () => new Vector2(PanelTitled.GetClientLength(width), 0),
                BackgroundTexture = UIManager.Icon16Background,
                //Anchor = Vector2.UnitX,
                LeftClickAction = RemoveOrder
            };
            btnClose.ShowOnParentFocus(true);
            btnClose.AnchorNew = Anchors.Right | Anchors.Top;
            this.AddControls(btnClose);

            var btnMinus = new Button("-", Minus, Button.DefaultHeight) { Location = this.ModeCBox.TopRight };
            var btnPlus = new Button("+", Plus, Button.DefaultHeight) { Location = btnMinus.TopRight };
            this.LabelAmount = new Label(()=>this._amountPredicted.ToString()) { Location = btnPlus.TopRight };

            this.AddControls(btnMinus, btnPlus, this.LabelAmount);

            //this.DetailsGui = this.DetailsGui ??= new CraftOrderDetailsGui(this);

            this.btnDetails = new Button("Details", ToggleDetails);
            //this.AddControls(btnDetails.AnchorToBottomRight());
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
            //this.btnClose.Location.X = this.Width - this.btnClose.Width;
            //this.btnDetails.Location = new(this.Width - this.btnDetails.Width, this.Height - this.btnDetails.Height);
        }
        //public override bool Show()
        //{
        //    this.Settings.Workstation.Map.Events.ListenTo<CraftOrderUpdatedEvent>(onCraftOrderModified);
        //    return base.Show();
        //}
        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{
        //    this.DrawHighlight(sb, Color.Blue * .5f);
        //    base.Draw(sb, viewport);
        //}

        private void onCraftOrderModified(CraftOrderUpdatedEvent e)
        {
            if (this.Settings == e.Order)
            {
                this._modePredicted = e.Order.Mode;
                this._amountPredicted = e.Order.Amount;
                //this.LabelAmount.Text = e.Order.Amount.ToString();
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
            //CraftingManagerOld.WriteOrderModifyPriority(Client.Instance.OutgoingStreamUnreliable, this, p);
        }
        void RemoveOrder()
        {
            PacketsCrafting.SendPlayerDeletedOrder(this.Settings.Workstation.Parent.Map, this.Settings);
        }
        void Minus()
        {
            //this.LabelAmount.Text = $"{Math.Max(0, this.Settings.Amount - 1)}"; // client prediction
            this._amountPredicted--;
            PacketsCrafting.SendPlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, -1, this.Settings.Mode);
        }
        void Plus()
        {
            //this.LabelAmount.Text = $"{this.Settings.Amount + 1}"; // client prediction
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
