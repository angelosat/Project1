using Project1.Framework.Base;

namespace Start_a_Town_
{
    public class DamageTypeDef(string name) : Def(name)
    {
    }
    public static class DamageTypeDefOf
    {
        public static readonly DamageTypeDef Physical = new("Physical");
        public static readonly DamageTypeDef Elemental = new("Elemental");
    }
    public class DamageDef(string name, DamageTypeDef damageType) : Def(name)
    {
        public readonly DamageTypeDef DamageType = damageType;
    }
    public static class DamageDefOf
    {
        public static readonly DamageDef Digging = new("Digging", DamageTypeDefOf.Physical);
        public static readonly DamageDef Mining = new("Mining", DamageTypeDefOf.Physical);
        public static readonly DamageDef Chopping = new("Chopping", DamageTypeDefOf.Physical);
        public static readonly DamageDef Tilling = new("Tilling", DamageTypeDefOf.Physical);
        public static readonly DamageDef Slashing = new("Slashing", DamageTypeDefOf.Physical);
        public static readonly DamageDef Piercing = new("Piercing", DamageTypeDefOf.Physical);
        public static readonly DamageDef Blunt = new("Blunt", DamageTypeDefOf.Physical);
        public static readonly DamageDef Sawing = new("Sawing", DamageTypeDefOf.Physical);

        public static readonly DamageDef Fire = new("Sawing", DamageTypeDefOf.Elemental);
        public static readonly DamageDef Ice = new("Sawing", DamageTypeDefOf.Elemental);
        public static readonly DamageDef Poison = new("Sawing", DamageTypeDefOf.Elemental);
        public static readonly DamageDef Acid = new("Sawing", DamageTypeDefOf.Elemental);
        public static readonly DamageDef Electricity = new("Sawing", DamageTypeDefOf.Elemental);
    }
}
