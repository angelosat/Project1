namespace Start_a_Town_
{
    internal class InteractionRepairLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            Resource _durabilityCached;
            Resource DurabilityCached => this._durabilityCached ??= this.Target.Object.Resources[ResourceDefOf.Durability];
            public override float ProgressPercentage => this.DurabilityCached.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        internal override void OnProgressAdded(Interaction i, int delta)
        {
            var actor = i.Actor;
            var item = i.Target.Object;
            if (actor.Net.IsClient) return;
            var durability = item.Resources[ResourceDefOf.Durability];
            durability.ApplyDelta(1);
        }
    }
}
