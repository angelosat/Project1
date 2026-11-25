using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.World.Combat;
using System;

namespace Project1.Core.Stats;

sealed class StatResolver_MeleeHitChance : StatResolver
{
    const int _min = 5, _fromSkill = 85, _fromDex = 5;
    const float _k = 20f;
    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        //var meleeSkill = actor.Skills.GetLevel(SkillDefOf.Melee);
        var skill = CombatSystem.GetWeaponSkill(actor);
        var meleeSkill = actor.Skills.GetLevel(skill);
        var dexterity = actor.Attributes.GetLevel(AttributeDefOf.Dexterity);

        var s = (float)Math.Clamp(meleeSkill, 0, 100);
        s /= 100f;
        var d = Math.Max(0, dexterity);
        var h = _min + _fromSkill * s + _fromDex * s * d / (d + _k);
        return h;
    }
}

sealed class StatResolver_SpellHitChance : StatResolver
{
    const int _min = 5, _fromAttribute = 90;
    const float _k = 20f;
    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        var intelligence = actor.Attributes.GetLevel(AttributeDefOf.Intelligence);
        var d = Math.Max(0, intelligence);
        var h = _min + _fromAttribute * d / (d + _k);
        return h;
    }
}
