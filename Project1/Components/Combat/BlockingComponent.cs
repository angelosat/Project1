using System;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    [Obsolete]
    class BlockingComponent : EntityComponent
    {
        public override string Name { get; } = "Blocking"; 
        public override object Clone()
        {
            return new BlockingComponent();
        }
        Animation Animation;
        public bool Active;
        public void Start(GameObject parent)
        {
            if (this.Active)
                return;
            this.Active = true;
            // TODO apply damage reduction
            this.Animation = Animation.Block;
            parent.AddAnimation(this.Animation);
            parent.GetComponent<MobileComponent>().ToggleBlock(true); // TODO: create a new movement state and set it in the mobile component?
        }
        public void Stop(GameObject parent)
        {
            this.Active = false;
            // TODO remove damage reduction
            this.Animation.FadeOut();
            parent.GetComponent<MobileComponent>().ToggleBlock(false);
        }
    }
}
