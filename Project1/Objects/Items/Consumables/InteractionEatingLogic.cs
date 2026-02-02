using Project1.Framework.Interactions;

namespace Start_a_Town_
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
