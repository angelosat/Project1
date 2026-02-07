using Project1.Core.Entities;
using Project1.Core.Interactions;

namespace Project1.Core.Interactions
{
    class InteractionEatingLogic : InteractionLogic 
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            if (actor.Net.IsClient) return;
            var foodItem = i.Context.Target.Object;
            var foodEffects = foodItem.GetComponent<ConsumableComponent>().EffectsNew;
            foreach (var f in foodEffects)
                actor.Effects.Apply(f);
        }
    }
}
