using Project1.Core.Interactions;
using System;

namespace Project1.Core.Towns.Shops;

sealed class InteractionBrowseProduct : InteractionLogic
{
    sealed class Context : InteractionContext
    {
        internal ShoppingList ShoppingList => field ??= this.Actor.ShoppingList;
        //public override float ProgressPercentage => this.ShoppingList.GetInterestPercentage(this.Target.Entity);
        //public override float ProgressPercentage => this.Actor.Net.IsClient ? 0 : this.ShoppingList.GetInterestPercentage(this.Target.Entity);
        // public override float ProgressPercentage => this.ShoppingList.GetInterestPercentage(this.Target.Entity) >= 1 ? 1 : 0;
    }
    protected override InteractionContext CreateContextInternal()
        => new Context();
    //internal override void OnFinish(Interaction i)
    //{
    //    var actor = i.Actor;
    //    if (actor.Net.IsClient)
    //        return;
    //    var item = i.Target.Entity;
    //    var prefs = actor.AI.State.ItemPreferences;
    //    var typedCtx = (Context)i.Context;
    //    //var list = actor.Map.Town.ShopManager.GetShoppingListEmpty(actor);
    //    var list = typedCtx.ShoppingList;
    //    var result = prefs.EvaluateAndRegister(item);
    //    if (result.Roles.Length == 0)
    //        return;
    //    var maxScore = result.Roles.Max(r => r.Score);
    //    var impulseThresholdPlaceholder = 2;
    //    var isImpulse = maxScore >= impulseThresholdPlaceholder;
    //    list.Register(item, maxScore, isImpulse);
    //    //actor.AI.State.Log.Write($"Considered product {result} (isImpulse: {isImpulse}");
    //    actor.AI.State.Log.Write($"Potential buy: {item.RefId}:{item.Name} maxScore: {maxScore} isImpulse: {isImpulse}");
    //}
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var item = i.Target.Entity;
        //var prefs = actor.AI.State.ItemPreferences;
        var typedCtx = (Context)i.Context;
        var list = typedCtx.ShoppingList;
        //var result = prefs.EvaluateAndRegister(item);
        var result = list.GetCachedResult(item);
        actor.AI.State.Knowledge.Register(item, result);
        if (result.Roles.Length == 0)
            return;
        //var maxScore = result.Roles.Max(r => r.Score);
        var maxScore = (int)list.GetInterest(item);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxScore);
        list.Register(item, maxScore);
        actor.AI.State.Log.Write($"Browsed: {item.RefId}: {item.Name} interest: {maxScore} isImpulse: {isImpulse}");
    }
    internal override void OnTick(Interaction i)
        //=> ((Context)i.Context).ShoppingList.AddInterest(i.Target.Entity, 1);
        => ((Context)i.Context).ShoppingList.AddInterestPercentage(i.Target.Entity, 1f / i.Progress.Max);
}
