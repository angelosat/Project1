using Project1.Core.Entities.Actors;
using Project1.Framework.UI;
using Project1.Core.Systems.Recipes;
using Project1.Framework.Helpers;

namespace Project1.Core.UI
{
    internal class Skills_Gui : SelectionBoundControl// GroupBox, ISelectionBound
    {
        readonly ListBoxNoScroll GuiList;
        public Skills_Gui()
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
            //this.AddControls(this.GuiList);
            this.AddControlsVertically(this.GuiList, new RecipesComp_Gui(actor.GetComponent<RecipesComp>()).Layout(200, 200).ToPanelLabeled("Recipe masteries"));
        }
    }
}
