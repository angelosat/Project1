using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class NeedsMoodsUI : GuiBuilder
    {
        GroupBox BoxNeeds, BoxMood;
        public NeedsMoodsUI()
        {
            
        }
        public NeedsMoodsUI(Entity entity) : base(entity)
        {
        }

        protected override void Build()
        {
            this.BoxNeeds = new GroupBox();
            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            this.Name = "Needs";
            var actor = this.Entity as Actor;
            var needs = actor.GetComponent<NeedsComponent>();
            var mood = actor.GetComponent<MoodComp>();

            this.BoxNeeds = new GroupBox();
            needs.GetUI(actor, this.BoxNeeds);

            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            mood.GetInterface(actor, this.BoxMood);

            this.Controls.Add(this.BoxNeeds, this.BoxMood);
        }
      
        public void Refresh(GameObject entity)
        {
            var needs = entity.GetComponent<NeedsComponent>();
            var mood = entity.GetComponent<MoodComp>();

            this.Controls.Clear();

            this.BoxNeeds.ClearControls();
            this.BoxMood.ClearControls();

            this.BoxNeeds.AddControls(needs.NewGui());
            this.BoxMood.AddControls(mood.NewGui());

            this.BoxMood.Location = this.BoxNeeds.TopRight;

            this.Controls.Add(this.BoxNeeds, this.BoxMood);

            this.Tag = entity;

            this.GetWindow()?.Title = $"{entity.Name} needs";

            this.Validate(true);
        }
        protected override GuiBuilder BuildFor(Entity entity) => new NeedsMoodsUI(entity);
      
        public Control Refresh(Actor actor)
        {
            this.Refresh(actor as GameObject);
            return this;
        }
    }
}
