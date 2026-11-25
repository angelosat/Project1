using Microsoft.Xna.Framework;
using Project1.Core.Systems.Stats;
using Project1.Framework;

namespace Project1.Core.Systems.Magic;

[EnsureStaticCtorCall]
static public class SpellSchoolDefOf
{
    static public readonly SpellSchoolDef Common = new("Common", null, Color.White);
    static public readonly SpellSchoolDef Holy = new("Holy", DamageDefOf.Holy, new(255, 255, 64, 255));
    static public readonly SpellSchoolDef Fire = new("Fire", DamageDefOf.Fire, new(255, 0, 0, 255));
    static SpellSchoolDefOf()
    {
        Def.Register(typeof(SpellSchoolDefOf));
    }
}
