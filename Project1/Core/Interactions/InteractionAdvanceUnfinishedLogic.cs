using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Resources;
using System;

namespace Project1.Core.Interactions
{
    sealed class InteractionAdvanceUnfinishedLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            internal Entity UnfinishedItem => field ??= this.Workstation.GetUnfinishedItem();
            internal UnfinishedItemComp UnfinishedComp => field ??= this.UnfinishedItem?.GetComponent<UnfinishedItemComp>();
            internal IResourceView Assembly => field ??= this.UnfinishedItem.Resources.ViewOld(ResourceDefOf.Assembly);
            internal ResourcesComponent Resources => field ??= this.UnfinishedItem?.GetComponent<ResourcesComponent>();
            internal BlockWorkstationComp Workstation => field ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            public override float ProgressBarPercentage => this.Assembly?.Percentage ?? 0;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanPerform(InteractionContext ctx)
            => ctx.Actor.Map.Town.CraftingManager.CanContinueItem(ctx.Actor, ((Context)ctx).UnfinishedComp);// .GetContract(ctx.Actor)?.IsValid ?? false;
        public override void ApplyWork(InteractionContext ctx, int workAmount)
        {
            var actor = ctx.Actor;
            if (actor.Net.IsClient)
                return;
            var ctxTyped = (Context)ctx;
            ArgumentNullException.ThrowIfNull(ctxTyped.UnfinishedItem);
            ctxTyped.Assembly.ApplyDelta(workAmount);
        }
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            if (actor.Net.IsClient)
                return;
            var ctxTyped = (Context)i.Context;
            var unfinishedItem = ctxTyped.UnfinishedItem;
            var orderId = unfinishedItem.GetComponent<UnfinishedItemComp>().OrderId;
            var map = actor.Map;
            var creationReq = ctxTyped.UnfinishedComp.GetCreationRequest();
            var ctx = i.Context as Context;
            foreach (var pair in ctx.UnfinishedComp.MaterialBindings)
                creationReq.OverrideMaterial(pair.Key, pair.Value);
            var product = creationReq.Create();
            product.Author = actor;

            map.Spawn(product, unfinishedItem.Global, unfinishedItem.Velocity);
            map.World.DisposeEntity(unfinishedItem);
            var order = map.Town.CraftingManager.GetOrder(orderId);
            order.CompletedBy(actor);
        }
    }
}
