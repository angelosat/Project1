using Project1.Core.Entities;
using Project1.Core.Interactions;

namespace Project1.Core.Interactions
{
    internal class InteractionEquipLogic : InteractionLogic
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
    //class InteractionEquip : InteractionPerpetual
    //{
    //    static public int ID = "Equip".GetHashCode();

    //    public InteractionEquip()
    //        : base("Equip")
    //    {
    //        this.CrossFadeAnimationLength = 25;
    //    }

    //    protected override void Done()
    //    {
    //        if (this.Actor.Net.IsClient)
    //            return;
    //        var a = this.Actor;
    //        var t = this.Target;
    //        //GearComponent.EquipToggle(a, t.Object as Entity);
    //        this.Actor.Gear.EquipToggle(t.Object as Entity);
    //        this.Finish();
    //    }
    //}
}
