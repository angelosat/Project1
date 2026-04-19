using Project1.Framework;
using Project1.Core.Needs;
using Project1.Core.Systems.Tools;
using Project1.Core.Resources;

namespace Project1.Core.Systems.ItemRoles;

[EnsureStaticCtorCall]
internal class ItemRoleContextDefOf
{
    public static readonly ItemRoleContextDef Tool = new("Tool", typeof(ToolUseDef), typeof(ItemRoleToolWorker));
    public static readonly ItemRoleContextDef Need = new("Need", typeof(NeedDef), typeof(ItemRoleNeedWorker));
    public static readonly ItemRoleContextDef Nutrition = new("Nutrition", null, typeof(ItemRoleNutritionWorker));
    public static readonly ItemRoleContextDef Cash = new("Cash", null, typeof(ItemRoleCash));
    public static readonly ItemRoleContextDef TownScroll = new("TownScroll", null, typeof(ItemRoleTownScroll));
    //public static readonly ItemRoleContextDef Potion = new("Potion", null, typeof(ItemRolePotion));
    public static readonly ItemRoleContextDef Fortify = new("Fortify", typeof(ResourceDef), typeof(ItemRoleFortify));
    public static readonly ItemRoleContextDef Restore = new("Restore", typeof(ResourceDef), typeof(ItemRoleRestore));

    static ItemRoleContextDefOf()
    {
        Def.Register(typeof(ItemRoleContextDefOf));
    }
}
