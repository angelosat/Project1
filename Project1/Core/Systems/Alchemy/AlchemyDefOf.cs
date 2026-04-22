using Project1.Core.Skills;
using Project1.Framework;
using Project1.Framework.UI;

namespace Project1.Core.Systems.Alchemy;

[EnsureStaticCtorCall]
internal static class AlchemyDefOf
{
    public readonly static SkillDef Skill = new("Alchemy")
    {
        Description = "Scribing description",
        Icon = new Icon(UIManager.Icons32, 12, 32)
    };
    static AlchemyDefOf()
    {
        Def.Register(typeof(AlchemyDefOf));
    }
}
