using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Skills;
using System;

namespace Project1.Core.Systems.Stats.Resolvers
{
    sealed class StatResolver_Unarmed : StatResolver
    {
        static readonly int BaseValue = 50; // 1;
        public override float CalculateActorStat(StatDef def, Actor actor)
        {
            var strength = actor.Attributes.Get(AttributeDefOf.Strength);
            var melee = actor.Skills.GetLevel(SkillDefOf.Melee);
            var min = BaseValue;
            var final = strength * (1 + melee / 100f);
            return Math.Max(min, final);
        }
    }
}
