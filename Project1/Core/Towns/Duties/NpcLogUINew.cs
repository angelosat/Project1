using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Duties;

class NpcLogUINewNew : SelectionBoundControl
{
    static int timeWidth = (int)UIManager.Font.MeasureString("HH:mm:ss").X;
    private TableScrollableCompact<AILog.Entry> Table;
    //= new TableScrollableCompact<AILog.Entry>()
    //            .AddColumn(null, "Time", timeWidth, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
    //            //.AddColumn(null, "Description", 200, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text), 200), 0);
    //            .AddColumn(null, "Description", 200, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNewNew(e.Text), 200), 0);

    public NpcLogUINewNew()
    {
        //var scrollbox = new ScrollableBoxNewNewNew(this.Table.TotalWidth, 300, ScrollModes.Vertical) { Autoscroll = true };
        //scrollbox.AddControls(this.Table);
        //this.AddControls(scrollbox.ToPanel());
    }

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;
        this.Table.Bind(actor.AI.State.Log.Inner);
    }

    public override void OnLayout(int availableWidth, int availableHeight)
    {
        this.ClearControls();
        var descWidth = availableWidth - timeWidth;
        this.Table = new TableScrollableCompact<AILog.Entry>()
                .AddColumn(null, "Time", timeWidth, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                .AddColumn(null, "Description", descWidth, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNewNew(e.Text), descWidth), 0);
        if(this.CurrentSelection is Actor actor)
            this.Table.Bind(actor.AI.State.Log.Inner);
        this.Controls.Add(this.Table);
    }
}
