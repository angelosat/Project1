using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class OrderSettings : IListable
    {
        public enum CraftMode
        {
            FixedAmount,       // Craft X times
            StockpileLimit,    // Craft until stockpile has at least X
            Infinite           // Craft forever
        }
        static public CraftMode[] AllModes = [CraftMode.FixedAmount, CraftMode.StockpileLimit, CraftMode.Infinite];
        public CraftMode Mode;
        int _amount;
        public int Amount//; // X for FixedAmount or StockpileLimit, ignored for Infinite
        {
            get => this._amount;
            set => this._amount = Math.Max(value, 0);
        }
        public bool Enabled;

        public EntityCreationRequest Target { get; init; }

        // Explicit actor restriction
        public HashSet<int> AllowedActors = [];

        // Minimum skill requirement
        public int SkillFilter;

        public int Id { get; }
        public SkillDef Skill { get; init; }
        public MaterialRefinementDef Refinement { get; init; }
        //public IntVec3 OwnerPosition { get; init; }
        public BlockEntityCompWorkstation Workstation { get; init; }
        public string Label => this.Refinement.Label;

        //// Optional input constraints
        //public Dictionary<MaterialTypeDef, int> RequiredInputs = [];

        public OrderSettings(int id, BlockEntityCompWorkstation owner, MaterialRefinementDef refinement)
        {
            this.Id = id;
            this.Skill = refinement.MaterialType.SkillToRefine;
            this.Refinement = refinement;
            this.Workstation = owner;
            //this.Target = new EntityCreationRequest(stage: mapping.Process)
        }
        public IEnumerable<IngredientRequirement> GetIngredientRequirements()
        {
            yield return new(ItemDefOf.Ingredient, this.Refinement.Source, 1, this.Workstation.Global.Above, RawMaterialSystem.MaterialsByType[this.Refinement.MaterialType]);
        }

        public bool CanActorPerform(Actor actor)
        {
            if (!this.Enabled) return false;
            if (this!.AllowedActors.Contains(actor.RefId)) return false;
            if (actor.Skills.GetSkill(this.Skill).Level < SkillFilter) return false;
            return true;
        }

        public bool ShouldQueue(int currentStock)
        {
            return Mode switch
            {
                CraftMode.FixedAmount => Amount > 0,
                CraftMode.StockpileLimit => currentStock < Amount,
                CraftMode.Infinite => true,
                _ => false
            };
        }

        public Control GetListControlGui()
        {
            return new OrderSettingsGui(this);
        }
        public void ChangePriority(int priorityDelta)
        {
            if (priorityDelta > 0)
                this.Workstation.MoveDown(this);
            else if (priorityDelta < 0)
                this.Workstation.MoveUp(this);
        }
        //public override string ToString()
        //{
        //    return $"{this}:{this.Refinement}";
        //}
    }
    internal class OrderSettingsGui : GroupBox
    {
        readonly OrderSettings Settings;
        Label LabelAmount;
        ComboBoxNewNew<OrderSettings.CraftMode> ModeCBox;
        OrderSettings.CraftMode _modePredicted;
        int _amountPredicted;
        Button btnDetails;
        private IconButton btnClose;

        public OrderSettingsGui(OrderSettings settings/*, int width*/)
        {
            this.Settings = settings;
            this._modePredicted = settings.Mode;
            this._amountPredicted = settings.Amount;
            //var box = new GroupBox
            //{
            //    BackgroundColor = UIManager.DefaultListItemBackgroundColor,
            //    MouseThrough = false
            //};
            this.BackgroundColor = UIManager.DefaultListItemBackgroundColor;
            this.MouseThrough = false;

            //var btnUp = new ButtonIcon(Icon.ArrowUp, MoveUp);
            //var btnDown = new ButtonIcon(Icon.ArrowDown, MoveDown) { Location = btnUp.BottomLeft };
            //this.AddControls(btnUp, btnDown);

            var orderName = new Label(settings.Label);// { Location = btnUp.TopRight };
            this.ModeCBox = new ComboBoxNewNew<OrderSettings.CraftMode>(OrderSettings.AllModes, 100, c => c.ToString(), ChangeFinishMode, () => this._modePredicted);// { Location = orderName.BottomLeft };
            this.ModeCBox.AnchorNew = Anchors.Bottom | Anchors.Left;

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

            this.btnDetails = new Button("Details");//, ToggleDetails);
            //this.AddControls(btnDetails.AnchorToBottomRight());
            this.btnDetails.AnchorNew = Anchors.Bottom | Anchors.Right;
            this.AddControls(btnDetails);

            //return box;

            //void ToggleDetails()
            //{
            //    if (DetailsWindow is null)
            //        DetailsWindow = new Window() { Movable = true, Closable = true };
            //    DetailsWindow.Client.ClearControls();
            //    DetailsWindow.Client.AddControls(this.DetailsGui);
            //    DetailsWindow.SetTitle(this.Name);
            //    if (DetailsWindow.Show())
            //        DetailsWindow.Location = UIManager.Mouse;
            //}

        }

        public override void OnLayout(int availableWidth, int availableHeight)
        {
            this.Width = availableWidth;
            this.Height = availableHeight;
            base.OnLayout(availableWidth, availableHeight);
            //this.btnClose.Location.X = this.Width - this.btnClose.Width;
            //this.btnDetails.Location = new(this.Width - this.btnDetails.Width, this.Height - this.btnDetails.Height);
        }
        public override bool Show()
        {
            this.Settings.Workstation.Map.Events.ListenTo<CraftOrderModifiedEvent>(onCraftOrderModified);
            return base.Show();
        }
        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{
        //    this.DrawHighlight(sb, Color.Blue * .5f);
        //    base.Draw(sb, viewport);
        //}

        private void onCraftOrderModified(CraftOrderModifiedEvent e)
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
            PacketPlayerCraftOrders.PlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 1, 0, this.Settings.Mode);
        }
        void MoveUp()
        {
            PacketPlayerCraftOrders.PlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, -1, 0, this.Settings.Mode);
        }
        void ChangeOrderPriority(bool p)
        {
            //CraftingManagerOld.WriteOrderModifyPriority(Client.Instance.OutgoingStreamUnreliable, this, p);
        }
        void RemoveOrder()
        {
            PacketPlayerCraftOrders.PlayerDeletedOrder(this.Settings.Workstation.Parent.Map, this.Settings);
        }
        void Minus()
        {
            //this.LabelAmount.Text = $"{Math.Max(0, this.Settings.Amount - 1)}"; // client prediction
            this._amountPredicted--;
            PacketPlayerCraftOrders.PlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, -1, this.Settings.Mode);
        }
        void Plus()
        {
            //this.LabelAmount.Text = $"{this.Settings.Amount + 1}"; // client prediction
            this._amountPredicted++;
            PacketPlayerCraftOrders.PlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, 1, this.Settings.Mode);
        }
        void ChangeFinishMode(OrderSettings.CraftMode mode)
        {
            //PacketCraftOrderChangeMode.Send(this, (int)obj.Mode);
            this._modePredicted = mode;
            PacketPlayerCraftOrders.PlayerModifiedOrder(this.Settings.Workstation.Parent.Map, this.Settings, 0, 0, mode);
        }

       
    }
    public class IngredientRequirement(ItemDef itemType, Def context, int quantity, IntVec3 workstationSlot, HashSet<MaterialDef> materials)
    {
        public readonly ItemDef ItemType = itemType;
        public readonly Def Context = context;
        public readonly int Quantity = quantity;
        public readonly IntVec3 Slot = workstationSlot;

        public readonly HashSet<MaterialDef> FilteredMaterials = materials;

        internal bool Matches(Entity e)
        {
            return e.Def == this.ItemType && e.Profile == this.Context && this.FilteredMaterials.Contains(e.Body.Material);
        }
        internal bool MatchesPartial(Entity e, out int missing)
        {
            if (e.Def == this.ItemType && e.Profile == this.Context && !this.FilteredMaterials.Contains(e.Body.Material))
            {
                missing = this.Quantity - e.StackSize;
                return true;
            }
            missing = -1;
            return false;
        }
        //public IngredientRequirement ToggleMaterial(MaterialDef mat)
        //{
        //    if (this.Materials.Contains(mat))
        //        this.Materials.Remove(mat);
        //    else
        //        this.Materials.Add(mat);
        //    return this;
        //}
    }
}
