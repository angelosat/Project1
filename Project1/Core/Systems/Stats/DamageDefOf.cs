using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.Stats;

public static class DamageDefOf
{
    //public static readonly DamageDef Digging = new("Digging", DamageTypeDefOf.Physical);
    //public static readonly DamageDef Mining = new("Mining", DamageTypeDefOf.Physical);
    //public static readonly DamageDef Chopping = new("Chopping", DamageTypeDefOf.Physical);
    //public static readonly DamageDef Tilling = new("Tilling", DamageTypeDefOf.Physical);
    public static readonly DamageDef Slashing = new("Slashing", DamageTypeDefOf.Physical);
    public static readonly DamageDef Piercing = new("Piercing", DamageTypeDefOf.Physical);
    public static readonly DamageDef Blunt = new("Blunt", DamageTypeDefOf.Physical);
    //public static readonly DamageDef Sawing = new("Sawing", DamageTypeDefOf.Physical);
    public static readonly DamageDef Fire = new("Fire", DamageTypeDefOf.Elemental);
    public static readonly DamageDef Ice = new("Ice", DamageTypeDefOf.Elemental);
    public static readonly DamageDef Poison = new("Poison", DamageTypeDefOf.Elemental);
    public static readonly DamageDef Acid = new("Acid", DamageTypeDefOf.Elemental);
    public static readonly DamageDef Electricity = new("Electricity", DamageTypeDefOf.Elemental);

    static DamageDefOf()
    {
        Def.Register(typeof(DamageDefOf));
    }
}
