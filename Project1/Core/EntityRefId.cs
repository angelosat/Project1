using Project1.Framework;

namespace Project1.Core;

public readonly record struct EntityRefId(int Value) : IStructIdInt<EntityRefId>
{
    internal static readonly EntityRefId Null = new(0);

    public static EntityRefId Create(int value) => new(value);

    public static implicit operator EntityRefId(int v) => new(v);
    public static implicit operator int(EntityRefId v) => v.Value;
}
