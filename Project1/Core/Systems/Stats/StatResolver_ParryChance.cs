using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Skills;
using Project1.Core.World.Combat;
using System;

namespace Project1.Core.Systems.Stats
{
    sealed class StatResolver_ParryChance : StatResolver
    {
        const int _pMin = 5, _pMax = 35;
        const float _k = 50f;
        public override float CalculateActorStat(StatDef def, Actor actor)
        {
            var skill = CombatSystem.GetWeaponSkill(actor);
            var level = actor.Skills.GetLevel(skill);
            var p = _pMin + _pMax * level / (level + _k);
            return p;
        }
    }
}
