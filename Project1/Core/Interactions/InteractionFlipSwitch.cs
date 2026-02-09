using Project1.Core.Blocks;

namespace Project1.Core.Interactions
{
    class InteractionFlipSwitch : Interaction
    {
        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            var e = a.Map.GetBlockEntity(t.Global);
            e.GetComp<BlockEntityCompSwitchable>().Toggle(a, t);
            this.Finish();
        }
    }
}
