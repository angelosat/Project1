using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class GearTypeDef(string name) : Def(name)
    {
    }
    [EnsureStaticCtorCall]
    internal static class GearTypeDefOf
    {
        public static readonly GearTypeDef Mainhand = new("Mainhand");
        public static readonly GearTypeDef Offhand = new("Offhand");
        public static readonly GearTypeDef Head = new("Head");
        public static readonly GearTypeDef Chest = new("Chest");
        public static readonly GearTypeDef Hands = new("Hands");
        public static readonly GearTypeDef Legs = new("Legs");
        public static readonly GearTypeDef Feet = new("Feet");
        static GearTypeDefOf()
        {
            Def.Register(typeof(GearTypeDefOf));
        }
    }
}
