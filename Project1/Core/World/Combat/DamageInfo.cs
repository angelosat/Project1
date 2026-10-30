using Project1.Core.Entities.Stats;

namespace Project1.Core.World.Combat;

record struct DamageInfo(int Value, DamageDef Type)
{
    static readonly public DamageInfo Empty = new();
}
