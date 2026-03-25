using Project1.Core.Interactions;
using System.Linq;

namespace Project1.Core.Towns.Shops;

sealed class InteractionBrowseProduct : InteractionLogic
{
    internal override void OnFinish(Interaction i)
    {
        var actor = i.Actor;
        if (actor.Net.IsClient)
            return;
        var item = i.Target.Entity;
        var prefs = actor.AI.State.ItemPreferences;
        var list = actor.Map.Town.ShopManager.GetShoppingListEmpty(actor);
        var result = prefs.EvaluateNew(item);
        if (result.Roles.Length == 0)
            return;
        var maxScore = result.Roles.Max(r => r.Score);
        var impulseThresholdPlaceholder = 2;
        var isImpulse = maxScore >= impulseThresholdPlaceholder;
            list.Register(item, maxScore, isImpulse);
        //actor.AI.State.Log.Write($"Considered product {result} (isImpulse: {isImpulse}");
        actor.AI.State.Log.Write($"Potential buy: {item.RefId}:{item.Name} maxScore: {maxScore} isImpulse: {isImpulse}");
    }
}
