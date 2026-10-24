namespace Project1.Core.Entities
{
    public sealed class CraftsmanshipDef : Def
    {
        public readonly int Tier;
        public readonly int Threshold;
        public CraftsmanshipDef(string name, int tier, int threshold) : base(name)
        {
            Tier = tier;
            Threshold = threshold;
        }
    }
}
