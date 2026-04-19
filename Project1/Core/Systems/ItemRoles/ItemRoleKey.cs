using Project1.Core.Needs;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Tools;

namespace Project1.Core.Systems.ItemRoles
{
    internal sealed record ItemRoleKey_Gear : ItemRoleKey
    {
        internal override ItemRoleContextDef Context => throw new System.NotImplementedException();
    }
    internal sealed record ItemRoleKey_TownScroll : ItemRoleKey
    {
        internal override ItemRoleContextDef Context => ItemRoleContextDefOf.TownScroll;
    }
    internal sealed record ItemRoleKey_Cash : ItemRoleKey
    {
        internal override ItemRoleContextDef Context => ItemRoleContextDefOf.Cash;
    }
    internal sealed record ItemRoleKey_Nutrition : ItemRoleKey
    {
        internal override ItemRoleContextDef Context => ItemRoleContextDefOf.Nutrition;
    }
    internal sealed record ItemRoleKey_Need(NeedDef Need) : ItemRoleKey
    {
        internal override ItemRoleContextDef Context => ItemRoleContextDefOf.Need;
    }
    internal sealed record ItemRoleKey_Tool(ToolUseDef ToolUse) : ItemRoleKey
    {
        internal override ItemRoleContextDef Context => ItemRoleContextDefOf.Tool;
    }
    //internal sealed record ItemRoleKey_Potion(EffectDef Effect, Def Target) : ItemRoleKey
    //{
    //    internal override ItemRoleContextDef Context => ItemRoleContextDefOf.Potion;
    //}
    internal abstract record ItemRoleKey 
    {
        internal abstract ItemRoleContextDef Context { get; }
    }
}
