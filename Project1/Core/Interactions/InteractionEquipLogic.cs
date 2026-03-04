using Project1.Core.Entities;

namespace Project1.Core.Interactions
{
    internal sealed class InteractionEquipLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var a = i.Context.Actor;
            if (a.Net.IsClient)
                return;
            var t = i.Context.Target;
            a.Gear.EquipToggle(t.Object as Entity);
        }
    }
}
