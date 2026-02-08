using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities;
using Project1.Core.Entities.Mood;
using Project1.Framework.UI;

namespace Project1.Core.UI
{
    class NeedsMoodsUINew : GroupBox, ISelectionBound
    {
        GroupBox BoxNeeds, BoxMood;

        public ISelectable CurrentSelection { get; set; }

        public NeedsMoodsUINew()
        {
            this.BoxNeeds = new();
            this.BoxMood = new();
            this.AddControls(this.BoxNeeds, this.BoxMood);
        }

        public void OnBind(ISelectable selectable)
        {
            if (selectable is TargetArgs target && target.Object is Actor actor)
            {
                var needs = actor.GetComponent<NeedsComponent>();
                var mood = actor.GetComponent<MoodComp>();

                this.BoxNeeds.ClearControls();
                this.BoxMood.ClearControls();

                needs.GetUI(actor, this.BoxNeeds);
                mood.GetInterface(actor, this.BoxMood);

                this.BoxMood.Location = this.BoxNeeds.TopRight;
                this.ClearControls();
                this.AddControls(this.BoxNeeds, this.BoxMood);

            }
        }

        //protected override void Build()
        //{
        //    this.BoxNeeds = new GroupBox();
        //    this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
        //    this.Name = "Needs";
        //    var actor = this.Entity as Actor;
        //    var needs = actor.GetComponent<NeedsComponent>();
        //    var mood = actor.GetComponent<MoodComp>();

        //    this.BoxNeeds = new GroupBox();
        //    needs.GetUI(actor, this.BoxNeeds);

        //    this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
        //    mood.GetInterface(actor, this.BoxMood);

        //    this.Controls.Add(this.BoxNeeds, this.BoxMood);
        //}

        //public void Refresh(GameObject entity)
        //{
        //    var needs = entity.GetComponent<NeedsComponent>();
        //    var mood = entity.GetComponent<MoodComp>();

        //    this.Controls.Clear();

        //    this.BoxNeeds.ClearControls();
        //    this.BoxMood.ClearControls();

        //    this.BoxNeeds.AddControls(needs.NewGui());
        //    this.BoxMood.AddControls(mood.NewGui());

        //    this.BoxMood.Location = this.BoxNeeds.TopRight;

        //    this.Controls.Add(this.BoxNeeds, this.BoxMood);

        //    this.Tag = entity;

        //    this.GetWindow()?.Title = $"{entity.Name} needs";

        //    this.Validate(true);
        //}
        //protected override GuiBuilder BuildFor(Entity entity) => new NeedsMoodsUI(entity);

        //public Control Refresh(Actor actor)
        //{
        //    this.Refresh(actor as GameObject);
        //    return this;
        //}

        
    }
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
