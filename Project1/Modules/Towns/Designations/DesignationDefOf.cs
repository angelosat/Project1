using Start_a_Town_.UI;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class DesignationDefOf
    {
        public static readonly DesignationDef Deconstruct = new("Deconstruct", typeof(DesignationWorkerDeconstruct), new QuickButton(new Icon(ItemContent.HammerFull), null, "Deconstruct") { HoverText = "Designate Deconstruction" });
        public static readonly DesignationDef Mine = new("Mine", typeof(DesignationWorkerMine), new QuickButton(new Icon(ItemContent.PickaxeFull), KeyBind.DigMine, "Mine") { HoverText = "Designate Mining" });
        public static readonly DesignationDef Switch = new("Switch", typeof(DesignationWorkerSwitch), new QuickButton('☞', null, "Switch") { HoverText = "Switch on/off" });
        static DesignationDefOf()
        {
            Def.Register(typeof(DesignationDefOf));
        }
    }
}
