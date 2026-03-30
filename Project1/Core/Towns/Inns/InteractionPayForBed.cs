using Project1.Core.Interactions;

namespace Project1.Core.Towns.Inns
{
    sealed class InteractionPayForBed : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Actor;
            var global = i.Target.Global;
            var count = i.Context.Count;
            var hauled = actor.Hauled;
            InteractionHelpers.TryDepositCarriedItemInsideBlockOrSpawn(actor, global, count);
            actor.Map.Town.InnManager.GetTransactionByGuest(actor).MarkPaid(hauled);
        }
    }
}
