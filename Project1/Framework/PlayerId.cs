namespace Project1.Framework
{
    public record struct PlayerId(int Value)
    {
        public static implicit operator int(PlayerId v) => (int)v;
        public static implicit operator PlayerId(int v) => new(v);
    }
}