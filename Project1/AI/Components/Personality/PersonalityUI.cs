using Start_a_Town_.UI;

namespace Start_a_Town_
{
    internal class PersonalityUI : GuiBuilder
    {
        public PersonalityUI() { }
        public PersonalityUI(Entity entity) : base(entity) { }
        protected override void Build()
        {
            var actor = this.Entity as Actor;
            var comp = actor.Personality;
            var gui = comp.NewGui();
            this.AddControls(gui);
        }
        protected override GuiBuilder BuildFor(Entity entity) => new PersonalityUI(entity);
    }
}
