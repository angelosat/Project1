using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorDamageBonus : StatResolver
    {
        readonly DamageDef Target;

        public StatResolver_ActorDamageBonus(DamageDef target)
        {
            Target = target;
        }

        public StatResolver_ActorDamageBonus()
        {

        }
        public override float CalculateItemStat(Entity owner)
        {
            return 0;
        }
        public override float CalculateActorStat(StatDef def, Actor actor)
        {
            return 0;
        }

        internal override string Format(StatDef def, Entity item)
        {
            return $"{Target.LabelReadable} damage";
        }
    }
}