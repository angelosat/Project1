using Project1.Core.Entities.Actors;
using Project1.Core.Entities;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    internal class SkillsUINew : SelectionBoundControl// GroupBox, ISelectionBound
    {
        readonly ListBoxNoScroll GuiList;
        public SkillsUINew()
        {
            this.GuiList = new();
        }

        protected internal override void OnBind(ISelectable selectable)
        {
            Build(selectable as Actor);
        }

        private void Build(Actor actor)
        {
            this.ClearControls();
            this.GuiList.Clear();
            GuiList.AddItems(actor.Skills.All);
            this.AddControls(this.GuiList);
        }
    }
}
