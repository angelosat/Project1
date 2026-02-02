using Project1.Framework.Interactions;
using Project1.Framework.Resources;

namespace Project1.Core.Interactions
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
