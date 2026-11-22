using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Skills;
using Project1.Core.Systems.Tools;
using System.Linq;

namespace Project1.Core.Systems.Stats;

internal sealed class StatResolver_WeaponLearning : StatResolver
{
    const float
        _b = 1f;
    static int
        _k = SkillRuntime.GetRequiredXp(100);
    static SkillDef[] CachedSkills => field ??= [.. Def.Get<GearProfileDef>().Where(g => g.WeaponSkill is not null).Select(s => s.WeaponSkill).Distinct()];
    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        var sum = CachedSkills.Sum(s => actor.Skills.GetSkill(s).CurrentXP);// - 1);
        var final = 1 + _b * sum / (sum + _k);
        final *= 100;
        return final;
    }
}