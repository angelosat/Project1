namespace Start_a_Town_
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
        public override object Clone()
        {
            return new InteractionFlipSwitch();
        }
    }
}
