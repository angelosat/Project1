namespace Project1.Core;

public record struct PlayerId(int Value)
{
    public static implicit operator PlayerId(int v) => new(v);
    public static implicit operator int(PlayerId v) => v.Value;
}
