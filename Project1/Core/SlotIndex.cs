namespace Project1.Core
{
    public readonly record struct SlotIndex(int Value)
    {
        internal static readonly SlotIndex Null = new(-1);
        public static implicit operator SlotIndex(int v) => new(v);
        public static implicit operator int(SlotIndex v) => v.Value;
    }
}
