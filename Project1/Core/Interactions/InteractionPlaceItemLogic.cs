using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Towns.Shops;
using Project1.Framework;
using System;

namespace Project1.Core.Interactions;

sealed class InteractionClaimBoughtItem : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransactionBySeller(this.Actor);
    }

    protected override InteractionContext CreateContextInternal() => new Context();
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var item = ctx.Target.Entity;
        actor.Inventory.HaulNew(item, item.StackSize);
        var manager = actor.AI.State.ItemPreferences;
        //var (role, score) = manager.GetPotential(item);
        //if (role is null)
        //    throw new Exception();
        //manager.Commit(role, item, score);
        manager.TryCommit(item);
        actor.Map.Town.ShopManager.FinishTransaction(actor);
    }
}
sealed class InteractionRingUpTransactionFinish : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransactionBySeller(this.Actor);
    }
    protected override InteractionContext CreateContextInternal() => new Context();
    public override bool CanPerform(InteractionContext ctx) => !((Context)ctx).Transaction.IsCancelled;
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        InteractionHelpers.TrySwapHauledItem(actor, ctx.Target.Entity, i.Count);
    }
}
sealed class InteractionRingUpTransaction : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransactionBySeller(this.Actor);
        internal int? Price => field ??= this.Actor.World.GetEntity(this.Transaction.Item).GetValueTotal();
        public override float ProgressPercentage => 0;
    }
    protected override InteractionContext CreateContextInternal() => new Context();
    public override bool CanPerform(InteractionContext ctx) => !((Context)ctx).Transaction.IsCancelled;
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var global = ctx.Target.Global;
        var count = ctx.Count;
        var typedCtx = (Context)ctx;
        var item = ctx.Target.Entity;
        actor.Inventory.HaulNew(item, item.StackSize);
        actor.Map.Town.ShopManager.RingUp(actor, item);
    }
}
sealed class InteractionPayTransaction : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShopTransaction Transaction => field ??= this.Actor.Map.Town.ShopManager.GetTransaction(this.Actor);
        internal int? Price => field ??= this.Actor.World.GetEntity(this.Transaction.Item).GetValueTotal();
        public override float ProgressPercentage => 0;
    }
    protected override InteractionContext CreateContextInternal() => new Context();
    public override bool CanPerform(InteractionContext ctx) => !((Context)ctx).Transaction.IsCancelled;
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var global = ctx.Target.Global;
        var count = ctx.Count;
        var hauled = actor.Hauled;
        var typedCtx = (Context)ctx;
        ArgumentNullException.ThrowIfNull(hauled);
        if (hauled.Def != ItemDefOf.Coins || hauled.StackSize < typedCtx.Price)
            throw new InvalidOperationException(); // or cancel transaction safely
        InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, global, count);
        actor.Map.Town.ShopManager.MarkPaid(actor, hauled);
    }
}
class InteractionSwapItemLogic : InteractionLogic
{
    class Context : InteractionContext
    {
        Cell _cachedCell;
        internal Cell Cell => _cachedCell ??= this.Target.Map.GetCell(this.Target.Global.Below());
    }
    protected override InteractionContext CreateContextInternal() => new Context();
    public override bool CanPerform(InteractionContext ctx) => ((Context)ctx).Cell.IsSolid();
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        InteractionHelpers.TrySwapHauledItem(actor, ctx.Target.Entity);
    }
}
class InteractionPlaceItemLogic : InteractionLogic
{
    class Context : InteractionContext
    {
        Cell _cachedCell;
        internal Cell Cell => _cachedCell ??= this.Target.Map.GetCell(this.Target.Global.Below());
    }
    protected override InteractionContext CreateContextInternal() => new Context();
    public override bool CanPerform(InteractionContext ctx) => ((Context)ctx).Cell.IsSolid();
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        var ctx = i.Context;
        var actor = ctx.Actor;
        if (actor.Net.IsClient)
            return;
        var global = ctx.Target.Global;
        var count = ctx.Count;
        var hauled = actor.Hauled;
        ArgumentNullException.ThrowIfNull(hauled);
        if (count > hauled.StackSize)
            throw new Exception();
        InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, global, count);
    }
}
class InteractionDepositLogic : InteractionLogic
{
    class Context : InteractionContext
    {
        internal Cell Cell => field ??= this.Target.Map.GetCell(this.Target.Global.Below());
    }
    protected override InteractionContext CreateContextInternal() => new Context();
    public override bool CanPerform(InteractionContext ctx) => ((Context)ctx).Cell.IsSolid();
    public override bool CanFinish(InteractionContext ctx) => this.CanPerform(ctx);
    internal override void OnFinish(Interaction i)
    {
        InteractionHelpers.DepositResource(i);
    }
}
