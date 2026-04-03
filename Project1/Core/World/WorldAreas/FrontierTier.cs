namespace Project1.Core.World.WorldAreas
{
    internal record struct Tier(int Value)
    {
        public static implicit operator int(Tier pos) => pos.Value;
        public static implicit operator Tier(int pos) => new(pos);
    }
    internal record struct FrontierTier(int Value)
    {
        public static implicit operator int(FrontierTier pos) => pos.Value;
        public static implicit operator FrontierTier(int pos) => new(pos);
    }
}
