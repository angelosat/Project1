namespace Project1.Core;

public readonly record struct ZoneId(int Value)
{
    internal static readonly ZoneId Null = new(0);
    public static implicit operator ZoneId(int v) => new(v);
    public static implicit operator int(ZoneId v) => v.Value;
}
