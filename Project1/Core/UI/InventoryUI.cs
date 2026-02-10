using Project1.Core.Entities.Actors;
using Project1.Core.Inventory;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    class InventoryUI : GroupBox, ISelectionBound
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
            var gearGui = actor.Gear.GetGUI();
            var gearPanel = gearGui.ToPanelLabeled("Gear");

            var panelSlots = new Panel(PanelWithVerticalTabs.DefaultSize);
            this.BoxSlots = new(panelSlots.ClientSize.Width, panelSlots.ClientSize.Height);
            var invgui = new InventoryContentsGui();
            invgui.Build(actor);
            this.BoxSlots.AddControls(invgui);

            panelSlots.AddControls(this.BoxSlots);
            var customizationClient = new GroupBox();
            colorsui = new GuiCharacterCustomization();

            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", () => PacketEditAppearance.Send(actor, colorsui.Colors), customizationClient.Width));

            var uicolors = new Window($"Edit colors", customizationClient) { Movable = true, Closable = true };

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsVertically(
                gearPanel,
                panelSlots,
                boxbtns);
            colorsui.SetTag(actor);
        }
    }
}
