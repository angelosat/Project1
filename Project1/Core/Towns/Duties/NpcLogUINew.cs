using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.Helpers;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Duties;

class NpcLogUINewNew : SelectionBoundControl
{
    private readonly TableScrollableCompact<AILog.Entry> Table = new TableScrollableCompact<AILog.Entry>()
                .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                .AddColumn(null, "Description", 400, (e) => new GroupBox().AddControlsLineWrap(Label.ParseNew(e.Text)), 0);

    public NpcLogUINewNew()
    {
        var scrollbox = new ScrollableBoxNewNewNew(this.Table.TotalWidth, 300, ScrollModes.Vertical);
        scrollbox.AddControls(this.Table);
        this.AddControls(scrollbox.ToPanel());
    }

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;
        this.Table.Bind(actor.AI.State.Log.Inner);
    }
}
