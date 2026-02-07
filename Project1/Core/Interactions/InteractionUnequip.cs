using Project1.Core.Interactions;

namespace Project1.Core.Interactions
{
    internal class InteractionUnequipLogic : InteractionLogic
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
    class InteractionUnequip : Interaction
    {
        public InteractionUnequip() : base("Unequipping", 0) { }

        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            a.Inventory.Unequip(t.Object);
        }
    }
}
