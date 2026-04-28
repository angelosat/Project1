using Project1.Framework;

namespace Project1.Core.Systems.Crafting;

public readonly record struct CraftingOrderId(int Value) : IStructIdInt<CraftingOrderId>
{
    public static readonly CraftingOrderId Null = new(0);

    public static CraftingOrderId Create(int value) => new(value);

    public static implicit operator CraftingOrderId(int v) => new(v);
    public static implicit operator int(CraftingOrderId v) => (int)v.Value;
}
