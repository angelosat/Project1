using Project1.Core.Entities.Actors;
using Project1.Core.Entities;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    internal class SkillsUINew : GroupBox, ISelectionBound
    {
        readonly ListBoxNoScroll GuiList;
        public ISelectable CurrentSelection { get; set; }
        public SkillsUINew()
        {
            this.GuiList = new();
        }

        public void OnBind(ISelectable selectable)
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
    internal class SkillsUI : GuiBuilder
    {
        public SkillsUI()
        {
            
        }
        public SkillsUI(Entity entity) : base(entity)
        {
            
        }
        protected override void Build()
        {
            var actor = this.Entity as Actor;
            this.AddControls(actor.Skills.NewGui());
        }

        protected override GuiBuilder BuildFor(Entity entity) => new SkillsUI(entity);
    }
}
