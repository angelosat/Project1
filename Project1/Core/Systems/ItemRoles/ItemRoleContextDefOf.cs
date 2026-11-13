using Project1.Core.Entities.Stats;
using Project1.Core.Needs;
using Project1.Core.Resources;
using Project1.Core.Systems.Tools;
using Project1.Framework;

namespace Project1.Core.Systems.ItemRoles;

[EnsureStaticCtorCall]
internal class ItemRoleContextDefOf
{
    public static readonly ItemRoleContextDef Tool = new("Tool", typeof(ToolUseDef), typeof(ItemRole_Tool), typeof(ItemRoleContextWorker_Tool));
    public static readonly ItemRoleContextDef Gear = new("Gear", typeof(StatDef), typeof(ItemRole_Gear), typeof(ItemRoleContextWorker_Gear));
    //public static readonly ItemRoleContextDef Equipment = new("Equipment", typeof(EquipmentDef), typeof(ItemRole_Equipment));
    public static readonly ItemRoleContextDef Need = new("Need", typeof(NeedDef), typeof(ItemRoleWorker_Need), typeof(ItemRoleContextWorker_Need));
    public static readonly ItemRoleContextDef Nutrition = new("Nutrition", null, typeof(ItemRoleWorker_Nutrition), typeof(ItemRoleContextWorker_Nutrition));
    public static readonly ItemRoleContextDef Cash = new("Cash", null, typeof(ItemRoleWorker_Cash), typeof(ItemRoleContextWorker_Cash));
    public static readonly ItemRoleContextDef TownScroll = new("TownScroll", null, typeof(ItemRoleWorker_TownScroll), typeof(ItemRoleContextWorker_Tool));
    //public static readonly ItemRoleContextDef Potion = new("Potion", null, typeof(ItemRolePotion));
    public static readonly ItemRoleContextDef Fortify = new("Fortify", typeof(ResourceDef), typeof(ItemRoleWorker_Fortify), typeof(ItemRoleContextWorker_Fortify));
    public static readonly ItemRoleContextDef Restore = new("Restore", typeof(ResourceDef), typeof(ItemRoleWorker_Restore), typeof(ItemRoleContextWorker_Restore));

    static ItemRoleContextDefOf()
    {
        Def.Register(typeof(ItemRoleContextDefOf));
    }
}
