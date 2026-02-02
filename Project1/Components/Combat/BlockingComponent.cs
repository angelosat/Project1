using System;
using Project1.Framework.Animations;
using Project1.Framework.Entities;

namespace Start_a_Town_.Components
{
    [Obsolete]
    class BlockingComponent : EntityComp
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
            throw new NotImplementedException("define AnimationDefOf.Block");
            //this.Animation = parent.SpriteComp.AddAnimation(AnimationDef.block);
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
