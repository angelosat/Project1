using System.Collections.Generic;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class AdventurerNeedsDefOf
    {
        static public readonly NeedCategoryDef NeedCategoryVisitor = new("Visitor")
        {
        };

        static public readonly NeedDef Guidance = new("Guidance", typeof(NeedGuidance), NeedCategoryVisitor);
        static public readonly NeedDef Trading = new("Trading", typeof(NeedTrading), NeedCategoryVisitor);
        static public readonly NeedDef Blessing = new("Blessing", typeof(NeedBlessing), NeedCategoryVisitor);
        static public readonly NeedDef InventorySpace = new("Inventory Space", typeof(NeedInventorySpace), NeedCategoryVisitor);
        static public readonly NeedDef Adventuring = new("Adventuring", typeof(NeedAdventure)) { TaskGiver = new TaskGiverDepart() };

        //static public readonly List<NeedDef> All = [Guidance, Trading, Blessing, InventorySpace];

        static AdventurerNeedsDefOf()
        {
            Def.Register(typeof(AdventurerNeedsDefOf));
        }

    }
}
