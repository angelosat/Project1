using Microsoft.Xna.Framework;
using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.Magic;

public sealed class SpellSchoolDef(string name, DamageDef damage, Color color) : Def(name)
{
    public readonly Color Color = color;
    public readonly DamageDef DamageType = damage;
}
