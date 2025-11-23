using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    public abstract class InteractionPerpetual : Interaction
    {
        public InteractionPerpetual()
        {
            this.Animation = new Animation(AnimationDef.Tool);
        }
        public InteractionPerpetual(string name)
            : base(name, 0)
        {
            this.RunningType = RunningTypes.Continuous;
            this.Animation = new Animation(AnimationDef.Tool);
        }
        
        public abstract void OnUpdate();
        internal override void InitAction()
        {
        }
        internal override void AfterLoad()
        {
            base.AfterLoad();
        }
        internal override void OnToolContact()
        {
            this.OnUpdate();
        }
    }
}
