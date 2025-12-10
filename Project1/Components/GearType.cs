using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class GearType : Def
    {
        public enum Types { 
            None, 
            Mainhand, Offhand, Head, Chest, Hands, Legs, Feet }
        public readonly Types ID;

        public static readonly GearType Mainhand = new GearType(Types.Mainhand, "Mainhand");
        public static readonly GearType Offhand = new GearType(Types.Offhand, "Offhand");
        public static readonly GearType Head = new GearType(Types.Head, "Head");
        public static readonly GearType Chest = new GearType(Types.Chest, "Chest");
        public static readonly GearType Hands = new GearType(Types.Hands, "Hands");
        public static readonly GearType Legs = new GearType(Types.Legs, "Legs");
        public static readonly GearType Feet = new GearType(Types.Feet, "Feet");

        public static readonly Dictionary<Types, GearType> Dictionary = new Dictionary<Types, GearType>() {
        { Types.Mainhand, GearType.Mainhand },
        { Types.Offhand, GearType.Offhand },
        { Types.Head, GearType.Head },
        { Types.Chest, GearType.Chest },
        { Types.Hands, GearType.Hands },
        { Types.Legs, GearType.Legs },
        { Types.Feet, GearType.Feet }
        };

        GearType(Types id, string name) : base(name)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
