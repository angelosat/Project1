using Project1.Core.Entities;
using Project1.Core.Interactions;
using System;
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
        //actor.AI.State.Knowledge.Register(item, result);
        if (result.Roles.Length > 0)
            list.Register(item, result.Roles.Max(r => r.Score));
    }
    //internal override void OnFinish(Interaction i)
    //{
    //    var actor = i.Actor;
    //    if (actor.Net.IsClient)
    //        return;
    //    var item = i.Target.Entity;
    //    var prefs = actor.AI.State.ItemPreferences;
    //    var list = actor.Map.Town.ShopManager.GetShoppingListPopulated(actor);
    //    //if (list.Dequeue() is not Entity entity || item != entity)
    //    //    throw new Exception();
    //    //prefs.EvaluateInt(item);
    //    var result = prefs.Evaluate(item);
    //    if (!result.Any())
    //        return;
    //    list.Register(item, result.Max(r => r.score));
    //    //actor.AI.State.Knowledge.Register(item, result.)
    //}
}
