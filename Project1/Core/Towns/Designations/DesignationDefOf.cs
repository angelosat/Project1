using Project1.Framework;
using Project1.Core.Assets;

namespace Project1.Core.Towns.Designations
{
    [EnsureStaticCtorCall]
    static class DesignationDefOf
    {
        public static readonly DesignationDef Deconstruct = new("Deconstruct", typeof(DesignationWorkerDeconstruct), ItemContent.HammerFull, "Deconstruct", "Designate Deconstruction", true);
        public static readonly DesignationDef Mine = new("Mine", typeof(DesignationWorkerMine), ItemContent.PickaxeFull, "Mine", "Designate Mining", true);
        public static readonly DesignationDef Switch = new("Switch", typeof(DesignationWorkerSwitch), '☞', "Switch","Switch on/off", true);

        public static readonly DesignationDef Chop = new("Chop", typeof(DesignationWorkerChop), ItemContent.AxeFull, "Chop", "Designate chopping", false);
        public static readonly DesignationDef Harvest = new("Harvest", typeof(DesignationWorkerHarvest), ItemContent.BerriesFull, "Harvest", "Designate harvesting", false);

        public static readonly DesignationDef Construct = new("Construct", typeof(DesignationWorkerConstruct), ItemContent.HammerFull, "Construct", "Designate Construction", true);

        static DesignationDefOf()
        {
            Def.Register(typeof(DesignationDefOf));
        }
    }
}
