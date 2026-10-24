using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorResourceBonus : StatResolver
    {
        public StatResolver_ActorResourceBonus(ResourceDef target)
        {
            Target = target;
        }

        public StatResolver_ActorResourceBonus()
        {

        }
        readonly ResourceDef Target;
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
            var target = (ResourceDef)statDef.TargetDef;
            return actorDef.Resources.Contains(target);
        }
        internal override string Format(StatDef def, Entity item)
        {
            return $"{Target.LabelReadable} effectiveness";
        }
    }
}
