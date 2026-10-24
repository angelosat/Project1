using Project1.Core.AI.Personality;
using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorTraitBonus : StatResolver
    {
        public StatResolver_ActorTraitBonus(TraitDef target)
        {
            Target = target;
        }

        public StatResolver_ActorTraitBonus()
        {

        }
        readonly TraitDef Target;
        public override float CalculateStat(Entity owner)
        {
            return 100;
        }
        public override float CalculateStatActor(StatDef def, Actor actor)
        {
            return 100;
        }
        internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
        {
            var target = (TraitDef)statDef.TargetDef;
            return actorDef.Traits.Contains(target);
        }
        internal override string Format(StatDef def, Entity item)
        {
            return $"{Target.LabelReadable} effectiveness";
        }
    }
}
