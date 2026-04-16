using Project1.Core.Entities.Actors;
using Project1.Framework.UI;
using Project1.Core.Mood;
using Project1.Core.Needs;

namespace Project1.Core.UI
{
    class NeedsMoodsUINew : SelectionBoundControl// GroupBox, ISelectionBound
    {
        GroupBox BoxNeeds, BoxMood;

        //public ISelectable CurrentSelection { get; set; }

        public NeedsMoodsUINew()
        {
            this.BoxNeeds = new();
            this.BoxMood = new();
            this.AddControls(this.BoxNeeds, this.BoxMood);
        }

        //public void OnBind(ISelectable selectable)
        protected internal override void OnBind(ISelectable selectable)
        {
            if (selectable is not Actor actor)
                return;
            //var actor = selectable as Actor;
            var needs = actor.GetComponent<NeedsComp>();
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
}
