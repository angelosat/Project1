using System.Collections.Generic;

namespace Start_a_Town_
{
    class VisitorNeedsDefOf
    {
        static public readonly NeedCategoryDef NeedCategoryVisitor = new("Visitor")
        {
        };

        static public readonly NeedDef Guidance = new("Guidance", typeof(NeedGuidance), NeedCategoryVisitor);
        static public readonly NeedDef Trading = new("Trading", typeof(NeedTrading), NeedCategoryVisitor);
        static public readonly NeedDef Blessing = new("Blessing", typeof(NeedBlessing), NeedCategoryVisitor);
        static public readonly NeedDef InventorySpace = new("Inventory Space", typeof(NeedInventorySpace), NeedCategoryVisitor);

        static public readonly List<NeedDef> All = new() { Guidance, Trading, Blessing, InventorySpace };

        static VisitorNeedsDefOf()
        {
            foreach (var d in All)
                Def.Register(d);
        }

        internal static void Init()
        {
        }
    }
}
