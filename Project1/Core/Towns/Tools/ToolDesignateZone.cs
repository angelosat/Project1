using Project1.Core.Screens;
using Project1.Core.Towns.Zones;
using Project1.Framework;

namespace Project1.Core.Towns.Tools;

class ToolDesignateZone : ToolZoningPositionsNew
{
    readonly ZoneDef Def;
    readonly int CurrentZoneID;
    int ClickedZoneID;
    int EditingZone => this.CurrentZoneID != 0 ? this.CurrentZoneID : this.ClickedZoneID;
    readonly string _helpText = "Hold control to clear designations";
    public override string HelpText => _helpText;
    readonly Town Town;
    public ToolDesignateZone()
    {

    }
    public ToolDesignateZone(Town town, ZoneDef def)
    {
        this.Def = def;
        this.CurrentZoneID = 0;
        this.Callback = this.Perform;
        this.Town = town;
    }
    void Perform(IntVec3 begin, int width, int height, bool isRemoval)
    {
        var end = new IntVec3(begin.X + width, begin.Y + height, begin.Z);
        Ingame.Instance.Events.Post(new PlayerAddingZoneEvent(this.Def, this.EditingZone, Ingame.Net.MainView.Map.ID, begin, end, isRemoval));
    }

    public override Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
    {
        base.MouseLeftPressed(e);
        var zone = this.Town.GetZoneAt(this.Begin);
        this.ClickedZoneID = zone != null ? zone.ID : 0;
        return Messages.Default;
    }
}
