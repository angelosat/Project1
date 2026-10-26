namespace Project1.Core
{
    public readonly record struct Tick(double Value)
    {
        public static implicit operator Tick(double v) => new(v);
        public static implicit operator double(Tick v) => v.Value;
    }
}
