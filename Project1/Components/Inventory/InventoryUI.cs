using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class InventoryUINew : GroupBox, ISelectionBound
    {
        GroupBox BoxSlots;
        GuiCharacterCustomization colorsui;

        public ISelectable CurrentSelection { get; set; }
        public void OnBind(ISelectable selectable)
        {
            if (selectable is TargetArgs target &&
                target.Object is Actor actor)
                Build(actor);
        }
        private void Build(Actor actor)
        {
            var panelSlots = new Panel(PanelWithVerticalTabs.DefaultSize);
            this.BoxSlots = new(panelSlots.ClientSize.Width, panelSlots.ClientSize.Height);
            this.BoxSlots.AddControls(actor.Inventory.Contents.Gui);

            panelSlots.AddControls(this.BoxSlots);
            var customizationClient = new GroupBox();
            colorsui = new GuiCharacterCustomization();

            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", () => PacketEditAppearance.Send(actor, colorsui.Colors), customizationClient.Width));

            var uicolors = new Window($"Edit colors", customizationClient) { Movable = true, Closable = true }; //Edit {this.Actor.Name}

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsVertically(
                panelSlots,
                boxbtns);
            colorsui.SetTag(actor);

        }

        //protected override void Build()
        //{
        //    this.Name = "Inventory";
        //    this.InitInvList();
        //    var actor = this.Entity as Actor;
        //    this.BoxSlots.ClearControls();
        //    this.BoxSlots.AddControls(actor.Inventory.Contents.Gui);
        //}

        //protected override GuiBuilder BuildFor(Entity entity) => new InventoryUI(entity);

        
    }
    class InventoryUI : GuiBuilder
    {
        static InventoryUI Instance;
        GroupBox BoxSlots;
        GuiCharacterCustomization colorsui;

        public InventoryUI()
        {
            
        }
        public InventoryUI(Entity entity) : base(entity)
        {
                
        }
        
        private void InitInvList()
        {
            var panelSlots = new Panel(PanelWithVerticalTabs.DefaultSize);
            this.BoxSlots = new(panelSlots.ClientSize.Width, panelSlots.ClientSize.Height);
            panelSlots.AddControls(this.BoxSlots);
            var customizationClient = new GroupBox();
            colorsui = new GuiCharacterCustomization();

            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", () => PacketEditAppearance.Send(this.Entity as Actor, colorsui.Colors), customizationClient.Width));

            var uicolors = new Window($"Edit colors", customizationClient) { Movable = true, Closable = true }; //Edit {this.Actor.Name}

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => (this.Entity as Actor).ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsVertically(
                panelSlots,
                boxbtns);
        }

        protected override void Build()
        {
            this.Name = "Inventory";
            this.InitInvList();
            var actor = this.Entity as Actor;
            this.BoxSlots.ClearControls();
            this.BoxSlots.AddControls(actor.Inventory.Contents.Gui);
            colorsui.SetTag(actor);
        }

        protected override GuiBuilder BuildFor(Entity entity) => new InventoryUI(entity);
    }
    
}
