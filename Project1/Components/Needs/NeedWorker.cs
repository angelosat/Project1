namespace Start_a_Town_
{
    public abstract class NeedWorker : MetricWorker
    {
        public sealed override void Tick(MetricWrapper wrapper)
        {
            var need = (Need)wrapper;
            this.TickExtra(need);
        }
        protected virtual void TickExtra(Need need) { }

    }
}
