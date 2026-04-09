using Microsoft.Xna.Framework;
using Project1.Framework;

namespace Project1.Core.Systems.Magic;

[EnsureStaticCtorCall]
static public class SpellSchoolDefOf
{
    static public readonly SpellSchoolDef Common = new("Common", Color.White);
    static public readonly SpellSchoolDef Holy = new("Holy", new(255, 255, 64, 255));
    static SpellSchoolDefOf()
    {
        Def.Register(typeof(SpellSchoolDefOf));
    }
}
