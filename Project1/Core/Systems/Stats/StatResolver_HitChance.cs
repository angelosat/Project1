using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Skills;
using System;

namespace Project1.Core.Stats;

sealed class StatResolver_HitChance : StatResolver
{
    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        var meleeSkill = actor.Skills.GetLevel(SkillDefOf.Melee);
        var dexterity = actor.Attributes.GetLevel(AttributeDefOf.Dexterity);

        var s = Math.Clamp(meleeSkill, 0, 1);
        var d = Math.Max(0, dexterity);
        var k = 20;
        var h = 5 + 85 * s + 5 * s * (d / (d + k));
        return h;
        //return meleeSkill / 5f + dexterity;
    }
}
