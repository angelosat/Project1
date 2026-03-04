namespace Project1.Core.Interactions
{
    internal sealed class InteractionUnequipLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var a = i.Context.Actor;
            if (a.Net.IsClient)
                return;
            var t = i.Context.Target;
            a.Inventory.Unequip(t.Object);
        }
    }
}
