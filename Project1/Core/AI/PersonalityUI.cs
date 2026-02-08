using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.AI
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
