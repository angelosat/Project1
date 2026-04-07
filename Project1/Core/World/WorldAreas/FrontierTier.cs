namespace Project1.Core.World.WorldAreas
{
    
    internal record struct FrontierTier(int Value)
    {
        public static implicit operator int(FrontierTier pos) => pos.Value;
        public static implicit operator FrontierTier(int pos) => new(pos);
    }
}
