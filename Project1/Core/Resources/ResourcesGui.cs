using Project1.Core.Entities.Actors;
using Project1.Core.UI;

namespace Project1.Core.Resources;

internal class ResourcesGui : SelectionBoundControl
{
    public ResourcesGui()
    {
        
    }
    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not Actor actor)
            return;
        this.Controls.Clear();
        foreach(var ctrl in actor.Resources.GetSelectionInfo())
            this.AddControlsBottomLeft(ctrl);
    }
}
