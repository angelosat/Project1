namespace Project1.Core;

public readonly record struct PacketId(int Value)
{
    public static implicit operator PacketId(int v) => new(v);
    public static implicit operator int(PacketId v) => v.Value;
}
