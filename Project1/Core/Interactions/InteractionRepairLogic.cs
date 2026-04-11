using Project1.Core.Resources;

namespace Project1.Core.Interactions;

sealed internal class InteractionRepairLogic : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal IResourceView Durability => field ??= this.Target.Object.Resources.View(ResourceDefOf.Durability);
        public override float ProgressBarPercentage => this.Durability.Percentage;
    }
    protected override InteractionContext CreateContextInt() => new Context();
    internal override void OnProgressAdded(Interaction i, int delta)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient) return;
        var ctx = (Context)i.Context;
        ctx.Durability.ApplyDelta(100);
    }
}
