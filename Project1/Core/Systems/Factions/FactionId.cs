using Project1.Core.AI.MetaRoles;
using Project1.Framework;

namespace Project1.Core.Systems.Factions;

public readonly record struct FactionId(int Value) : IStructIdInt<FactionId>
{
    internal static readonly FactionId Null = new(0);

    public static FactionId Create(int value) => new(value);

    public static implicit operator FactionId(int v) => new(v);
    public static implicit operator int(FactionId v) => v.Value;
}

public class FactionDef(string name, RoleMetaDef metaRole) : Def(name)
{
    public readonly RoleMetaDef RoleMeta = metaRole;
}

[EnsureStaticCtorCall]
public static class FactionDefOf
{
    public static readonly FactionDef Colony = new("Colony", RoleMetaDefOf.TownMember);
    public static readonly FactionDef Adventurers = new("Adventurers", RoleMetaDefOf.Adventurer);
    public static readonly FactionDef Bandits = new("Bandits", RoleMetaDefOf.Npc);

    static FactionDefOf()
    {
        Def.Register(typeof(FactionDefOf));
    }
}
