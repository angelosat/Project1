namespace Start_a_Town_
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

        protected abstract void OnUpdate();

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
