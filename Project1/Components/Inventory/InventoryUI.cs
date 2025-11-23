using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class InventoryUI : GroupBox
    {
        static InventoryUI Instance;
        //ScrollableBoxNewNew PanelSlots; 
        GroupBox BoxSlots;
        Actor Actor => this.Tag as Actor;
        GuiCharacterCustomization colorsui;

        public InventoryUI()
        {
            this.Name = "Inventory";
            this.InitInvList();
        }
      
        public InventoryUI Refresh(Actor actor)
        {
            this.Tag = actor;
            this.BoxSlots.ClearControls();
            this.BoxSlots.AddControls(actor.Inventory.Contents.Gui);
            colorsui.SetTag(actor);
            return this;
        }
        private void InitInvList()
        {
            //this.PanelSlots = new ScrollableBoxNewNew(256, 256);
            var panelSlots = new Panel(PanelWithVerticalTabs.DefaultSize);
            this.BoxSlots = new(panelSlots.ClientSize.Width, panelSlots.ClientSize.Height);
            panelSlots.AddControls(this.BoxSlots);
            var customizationClient = new GroupBox();
            colorsui = new GuiCharacterCustomization();

            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", () => PacketEditAppearance.Send(this.Actor, colorsui.Colors), customizationClient.Width));

            var uicolors = new Window($"Edit colors", customizationClient) { Movable = true, Closable = true }; //Edit {this.Actor.Name}

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => this.Actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsVertically(
                //this.BoxSlots,//.ToPanel(),
                panelSlots,
                boxbtns);
        }
        static public Control GetGui(Actor actor)
        {
            Window window;
            if (Instance is null)
            {
                Instance = new InventoryUI();
                window = new Window(Instance) { Closable = true, Movable = true };
                window.SnapToMouse();
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = $"{actor.Name} inventory";
            Instance.Refresh(actor);
            
            window.Validate(true);
            return window;
        }
      
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            if (target.Object is Actor actor && this.Tag != actor)
                GetGui(actor);
        }
    }
}
