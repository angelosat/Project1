using Project1.Core.Assets;
using Project1.Framework;

namespace Project1.Core.Towns.Designations
{
    [EnsureStaticCtorCall]
    static class DesignationDefOf
    {
        public static readonly DesignationDef Mine = new("Mine", typeof(DesignationWorkerMine), ItemContent.PickaxeFull, "Mine", "Designate Mining", TargetType.Cell);
        public static readonly DesignationDef Switch = new("Switch", typeof(DesignationWorkerSwitch), '☞', "Switch","Switch on/off", TargetType.Cell);

        public static readonly DesignationDef Chop = new("Chop", typeof(DesignationWorkerChop), ItemContent.AxeFull, "Chop", "Designate chopping", TargetType.Entity);
        public static readonly DesignationDef Harvest = new("Harvest", typeof(DesignationWorkerHarvest), ItemContent.BerriesFull, "Harvest", "Designate harvesting", TargetType.Entity);

        public static readonly DesignationDef Construct = new("Construct", typeof(DesignationWorkerConstruct), ItemContent.HammerFull, "Construct", "Designate Construction", TargetType.Cell) { IsManual = false };
        public static readonly DesignationDef Deconstruct = new("Deconstruct", typeof(DesignationWorkerDeconstruct), ItemContent.HammerHead, "Deconstruct", "Designate Deconstruction", TargetType.Cell);

        static DesignationDefOf()
        {
            Def.Register(typeof(DesignationDefOf));
        }
    }
}
