using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorAttributeBonus : StatResolver
    {
        public StatResolver_ActorAttributeBonus()
        {

        }
        readonly ResourceDef Target;
        public override float CalculateItemStat(Entity owner)
        {
            return 100;
        }
        public override float CalculateActorStat(StatDef def, Actor actor)
        {
            return 100;
        }
        internal override bool IsApplicable(StatDef statDef, ActorDnaDef actorDef)
        {
            var target = (AttributeDef)statDef.TargetDef;
            return actorDef.Attributes.Contains(target);
        }
        internal override string Format(StatDef def, Entity item)
        {
            return $"{def.TargetDef.LabelReadable} effectiveness";
        }
    }
}
