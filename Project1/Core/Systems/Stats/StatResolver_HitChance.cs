using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Skills;

namespace Project1.Core.Stats
{
    sealed class StatResolver_HitChance : StatResolver
    {
        public override float CalculateActorStat(StatDef def, Actor actor)
        {
            var meleeSkill = actor.Skills.GetLevel(SkillDefOf.Melee);
            var dexterity = actor.Attributes.GetLevel(AttributeDefOf.Dexterity);
            return meleeSkill / 5f + dexterity;
        }
    }
}
