using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.AI
{
    internal class PersonalityUI : SelectionBoundControl
    {
        protected internal override void OnBind(ISelectable selectable)
        {
            if (selectable is not Actor actor)
                return;
            var comp = actor.Personality;
            var gui = comp.NewGui();
            this.Controls.Clear();
            this.AddControls(gui);
        }
    }
}
