using Start_a_Town_.UI;

namespace Start_a_Town_
{
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
