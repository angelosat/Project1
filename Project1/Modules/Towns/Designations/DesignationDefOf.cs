using Start_a_Town_.UI;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class DesignationDefOf
    {
        //public static readonly DesignationDef Deconstruct = new("Deconstruct", typeof(DesignationWorkerDeconstruct), new QuickButton(new Icon(ItemContent.HammerFull), null, "Deconstruct") { HoverText = "Designate Deconstruction" });
        //public static readonly DesignationDef Mine = new("Mine", typeof(DesignationWorkerMine), new QuickButton(new Icon(ItemContent.PickaxeFull), KeyBind.DigMine, "Mine") { HoverText = "Designate Mining" });
        //public static readonly DesignationDef Switch = new("Switch", typeof(DesignationWorkerSwitch), new QuickButton('☞', null, "Switch") { HoverText = "Switch on/off" });

        public static readonly DesignationDef Deconstruct = new("Deconstruct", typeof(DesignationWorkerDeconstruct), ItemContent.HammerFull, "Deconstruct", "Designate Deconstruction", true);
        public static readonly DesignationDef Mine = new("Mine", typeof(DesignationWorkerMine), ItemContent.PickaxeFull, "Mine", "Designate Mining", true);
        public static readonly DesignationDef Switch = new("Switch", typeof(DesignationWorkerSwitch), '☞', "Switch","Switch on/off", true);

        public static readonly DesignationDef Chop = new("Chop", typeof(DesignationWorkerChop), ItemContent.AxeFull, "Chop", "Designate chopping", false);
        public static readonly DesignationDef Harvest = new("Harvest", typeof(DesignationWorkerHarvest), ItemContent.BerriesFull, "Harvest", "Designate harvesting", false);

        static DesignationDefOf()
        {
            Def.Register(typeof(DesignationDefOf));
        }
    }
}
