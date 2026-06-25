using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using Project1.Core.Networking.Inventory;
using Project1.Framework.Helpers;
using Project1.Framework.UI;

namespace Project1.Core.UI;

class InventoryUI : SelectionBoundControl// GroupBox, ISelectionBound
{
    //GroupBox BoxSlots;
    GuiCharacterCustomization colorsui;

    protected internal override void OnBind(ISelectable selectable)
    {
        if(selectable is Actor actor)
            Build(actor);
    }
    private void Build(Actor actor)
    {
        var gearGui = actor.Gear.GetGUI();
        var gearPanel = gearGui.ToPanelLabeled("Gear");

        var panelSlots = new Panel(PanelWithVerticalTabs.DefaultSize);
        var invgui = new Gui_ActorInventory();
        //this.BoxSlots = new(panelSlots.ClientSize.Width, panelSlots.ClientSize.Height);
        var BoxSlots = ScrollableBoxNewNewNew.FromWidth(invgui, invgui.RowWidth, panelSlots.ClientSize.Height);
        invgui.Build(actor);
        //BoxSlots.AddControls(invgui);

        panelSlots.AddControls(BoxSlots);
        var customizationClient = new GroupBox();
        colorsui = new GuiCharacterCustomization();

        customizationClient.AddControls(colorsui);
        customizationClient.AddControlsBottomLeft(new Button("Apply", () => PacketEditAppearance.Send(actor, colorsui.Colors), customizationClient.Width));

        var uicolors = new Window($"Edit colors", customizationClient) { Movable = true, Closable = true };

        var boxbtns = new GroupBox();
        var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
        var btnprefs = new Button("Item Preferences", () => actor.ItemPreferences.Gui.Toggle(), 128);
        boxbtns.AddControlsVertically(btncolors, btnprefs);

        var gearStats = new GearStatsPanel
        {
            AutoSize = false,
            Width = 100,
            Height = gearGui.Height
        };
        var gearStatsPanel = gearStats.ToPanelLabeled("Stats");

        var boxGearStats = new GroupBox();
        boxGearStats.AddControlsHorizontally(gearPanel, gearStatsPanel);

        this.AddControlsVertically(
            //gearPanel,
            //gearStatsPanel,
            boxGearStats,
            //panelSlots,
        BoxSlots.ToPanel(),
            boxbtns);
        colorsui.SetTag(actor);
    }
}

class GearStatsPanel : SelectionBoundControl
{
    GearComp Comp;

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;
        Build(actor);
    }

    private void Build(Actor actor)
    {
        this.Controls.Clear();
        this.Comp = actor.GetComponent<GearComp>();

        var statsCache = this.Comp.GetCachedStats();
        foreach (var (stat, value) in statsCache)
            this.AddControls(new LabelNew($"{stat}: {value}"));

        this.AlignTopToBottom();
    }

    public override void Update()
    {
        base.Update();
        if (this.Comp is null)
            return;
        if (this.Comp.StatsDirty)
            this.Build(this.Comp.Owner as Actor);
    }
}