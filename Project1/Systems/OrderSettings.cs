using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
        public int Amount; // X for FixedAmount or StockpileLimit, ignored for Infinite

        public bool Enabled;

        public EntityCreationRequest Target { get; init; }

        // Explicit actor restriction
        public HashSet<int> AllowedActors = [];

        // Minimum skill requirement
        public int SkillFilter;

        public SkillDef Skill { get; init; }
        public MaterialMappingDef Process { get; init; }
        public IntVec3 OwnerPosition { get; init; }

        public string Label => this.Process.Name;

        // Optional input constraints
        public Dictionary<MaterialTypeDef, int> RequiredInputs = [];

        public OrderSettings(BlockEntityCompWorkstation owner, MaterialMappingDef mapping)
        {
            this.Skill = mapping.MaterialType.SkillToRefine;
            this.Process = mapping;
            this.OwnerPosition = owner.Global;
            //this.Target = new EntityCreationRequest(stage: mapping.Process)
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
    }
    internal class OrderSettingsGui : GroupBox
    {
        readonly OrderSettings Settings;

        public OrderSettingsGui(OrderSettings settings)
        {
            this.Settings = settings;
            //var box = new GroupBox
            //{
            //    BackgroundColor = UIManager.DefaultListItemBackgroundColor,
            //    MouseThrough = false
            //};
            this.BackgroundColor = UIManager.DefaultListItemBackgroundColor;
            this.MouseThrough = false;

            var btnUp = new ButtonIcon(Icon.ArrowUp, MoveUp);
            var btnDown = new ButtonIcon(Icon.ArrowDown, MoveDown) { Location = btnUp.BottomLeft };
            this.AddControls(btnUp, btnDown);

            var orderName = new Label(settings) { Location = btnUp.TopRight };
            var comboFinishMode = new ComboBoxNewNew<OrderSettings.CraftMode>(OrderSettings.AllModes, 100, c => c.ToString(), ChangeFinishMode, () => settings.Mode) { Location = orderName.BottomLeft };

            this.AddControls(orderName,
                comboFinishMode);

            var btnClose = new IconButton(Icon.X) { LocationFunc = () => new Vector2(PanelTitled.GetClientLength(290), 0), BackgroundTexture = UIManager.Icon16Background };
            btnClose.Anchor = Vector2.UnitX;
            btnClose.LeftClickAction = RemoveOrder;
            btnClose.ShowOnParentFocus(true);
            this.AddControls(btnClose);

            var btnMinus = new Button("-", Minus, Button.DefaultHeight) { Location = comboFinishMode.TopRight };
            var btnPlus = new Button("+", Plus, Button.DefaultHeight) { Location = btnMinus.TopRight };
            this.AddControls(btnMinus, btnPlus);

            //this.DetailsGui = this.DetailsGui ??= new CraftOrderDetailsGui(this);

            var btnDetails = new Button("Details");//, ToggleDetails);
            this.AddControls(btnDetails.AnchorToBottomRight());

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
        void MoveDown()
        {
            ChangeOrderPriority(false);
        }
        void MoveUp()
        {
            ChangeOrderPriority(true);
        }
        void ChangeOrderPriority(bool p)
        {
            //CraftingManagerOld.WriteOrderModifyPriority(Client.Instance.OutgoingStreamUnreliable, this, p);
        }
        void RemoveOrder()
        {
            //PacketOrderRemove.Send(Client.Instance, this);
        }
        void Minus()
        {
            //CraftingManagerOld.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStreamUnreliable, this, -1);
        }
        void Plus()
        {
            //CraftingManagerOld.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStreamUnreliable, this, 1);
        }
        void ChangeFinishMode(OrderSettings.CraftMode mode)
        {
            //PacketCraftOrderChangeMode.Send(this, (int)obj.Mode);
        }
    }
}
