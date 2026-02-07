using Project1.Core.Base;

namespace Project1.Core.Attributes
{
    static class AttributeDefOf
    {
        public static readonly AttributeDef Strength = new("Strength", typeof(AttributeStrength));
        public static readonly AttributeDef Intelligence = new("Intelligence", typeof(AttributeIntelligence));
        public static readonly AttributeDef Dexterity = new("Dexterity", typeof(AttributeDexterity));
        static AttributeDefOf()
        {
            Def.Register(typeof(AttributeDefOf));
        }
    }
}
