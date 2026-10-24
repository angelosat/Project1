using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorSkillBonus : StatResolver
    {
        public StatResolver_ActorSkillBonus(SkillDef target)
        {
            Target = target;
        }

        public StatResolver_ActorSkillBonus()
        {

        }
        readonly SkillDef Target;
        public override float CalculateStat(Entity owner)
        {
            return 0;
        }
        public override float CalculateStatActor(StatDef def, Actor actor)
        {
            return 0;
        }
        internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
        {
            var target = (SkillDef)statDef.TargetDef;
            return actorDef.Skills.Contains(target);
        }
        internal override string Format(StatDef def, Entity item)
        {
            return $"{Target.LabelReadable} skill";
        }
    }
}
