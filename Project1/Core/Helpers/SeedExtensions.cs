namespace Project1.Core.Helpers
{
    public static class SeedExtensions
    {
        static public WorldSeed MixWith(this WorldSeed seed, WorldSeed other)
        {
            unchecked
            {
                return seed * 397 ^ other;
            }
        }
    }
}
