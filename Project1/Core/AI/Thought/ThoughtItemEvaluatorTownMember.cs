using Project1.Core.Entities;
using Project1.Core.Towns.Shops;
using System.Linq;

namespace Project1.Core.AI.Thought;

internal class ThoughtItemEvaluatorVisitor : ThoughtProcess
{
    public override void Tick(AIState state)
    {
        var actor = state.Owner;
        if (!actor.IsSpawned)
            return;
        var manager = state.ItemPreferences;
        var list = actor.Map.Town.ShopManager.GetShoppingListEmpty(actor);
        //if(list.GetResultsSorted().FirstOrDefault() is var best && best.item is Entity item)
        //{
        //    var placeholderThreshold = 2;
        //    if(best.score >= placeholderThreshold)
        //    {
        //        list.Impulse = item;
        //    }
        //}
        while (manager.DequeueUnevaluated() is Entity nextEntity)
        {
            if (!nextEntity.IsForSale())
                continue;
            list.Add(nextEntity);
        }
    }
}
internal class ThoughtItemEvaluatorTownMember : ThoughtProcess
{
    public override void Tick(AIState state)
    {
        if (!state.Owner.IsSpawned)
            return;
        var manager = state.ItemPreferences;
        if(manager.DequeueUnevaluated() is Entity item)
        {
            var result = manager.EvaluateNew(item);
            //state.Knowledge.Register(item, result);
            manager.TryPreCommit(item, result);
        }
    }
}
