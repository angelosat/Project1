using Project1.Core.Attributes;
using Start_a_Town_;

namespace Project1.Framework.Attributes
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
