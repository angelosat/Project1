namespace Project1.Framework.Interactions
{
    public abstract class InteractionPerpetual : Interaction
    {
        public InteractionPerpetual()
        {
        }
        public InteractionPerpetual(string name)
            : base(name, 0)
        {
            this.RunningType = RunningTypes.Continuous;
        }
        internal override void AfterLoad()
        {
            base.AfterLoad();
        }
    }
}
