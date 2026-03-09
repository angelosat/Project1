using Project1.Core.Blocks.Comps;

namespace Project1.Core.Interactions
{
    class InteractionFlipSwitchLogic : InteractionLogic
    {
        //public override void Perform()
        //{
        //    var a = this.Actor;
        //    var t = this.Target;
        //    var e = a.Map.GetBlockEntity(t.Global);
        //    e.GetComp<BlockEntityCompSwitchable>().Toggle(a, t);
        //    this.Finish();
        //}
        internal override void OnFinish(Interaction i)
        {
            var target = i.Target;
            var global = target.Global;
            var e = target.Map.GetBlockEntity(global);
            e.GetComp<BlockSwitchableComp>().Toggle();
        }
    }
}
