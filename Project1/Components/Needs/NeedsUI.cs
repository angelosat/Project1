using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    class NeedsUI : GroupBox
    {
        GroupBox BoxNeeds, BoxMood;
        public NeedsUI()
        {
            this.BoxNeeds = new GroupBox();
            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            this.Name = "Needs";

        }
       
        [Obsolete]
        public NeedsUI(GameObject entity)
        {
            var needs = entity.GetComponent<NeedsComponent>();
            var mood = entity.GetComponent<MoodComp>();

            this.BoxNeeds = new GroupBox();
            needs.GetUI(entity, this.BoxNeeds);

            this.BoxMood = new GroupBox() { Location = this.BoxNeeds.TopRight };
            mood.GetInterface(entity, this.BoxMood);

            this.Controls.Add(this.BoxNeeds, this.BoxMood);

        }
        public void Refresh(GameObject entity)
        {
            var needs = entity.GetComponent<NeedsComponent>();
            var mood = entity.GetComponent<MoodComp>();

            this.Controls.Clear();

            this.BoxNeeds.ClearControls();
            this.BoxMood.ClearControls();

            needs.GetUI(entity, this.BoxNeeds);

            mood.GetInterface(entity, this.BoxMood);

            this.BoxMood.Location = this.BoxNeeds.TopRight;

            this.Controls.Add(this.BoxNeeds, this.BoxMood);
            this.Validate(true);
        }
        static NeedsUI Instance;
        internal static Window GetGui(Actor actor)
        {
            Window window;

            if (Instance == null)
            {
                Instance = new NeedsUI();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = $"{actor.Name} needs";
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            Refresh(target.Object as Actor);
            return;
            var actor = target.Object as Actor;
            if (!actor?.Equals(this.Tag) ?? false)
            {
                GetGui(actor);
            }
        }
        public Control Refresh(Actor actor)
        {
            this.Refresh(actor as GameObject);
            return this;
        }
    }
}
