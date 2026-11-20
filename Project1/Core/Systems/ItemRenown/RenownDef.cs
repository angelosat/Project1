using Project1.Framework;

namespace Project1.Core.Systems.ItemRenown;

public class RenownDef(string name) : Def(name)
{
}

[EnsureStaticCtorCall]
public static class RenownDefOf
{
    public static readonly RenownDef Unknown = new("Unknown");
    public static readonly RenownDef Notable = new("Notable");
    public static readonly RenownDef Famed = new("Famed");
    public static readonly RenownDef Epic = new("Epic");
    public static readonly RenownDef Legendary = new("Legendary");
}
