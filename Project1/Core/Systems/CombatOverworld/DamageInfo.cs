using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.CombatOverworld;

record struct DamageInfo(int Value, DamageDef Type)
{
    static public DamageInfo Empty = new();
}
