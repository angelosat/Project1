using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatResolver_ActorResistanceBonus : StatResolver
    {
        readonly DamageDef Target;

        public StatResolver_ActorResistanceBonus(DamageDef target)
        {
            Target = target;
        }

        public StatResolver_ActorResistanceBonus()
        {

        }
        public override float CalculateStat(Entity owner)
        {
            return 0;
        }
        public override float CalculateStatActor(StatDef def, Actor actor)
        {
            return 0;
        }

        internal override string Format(StatDef def, Entity item)
        {
            return $"{Target.LabelReadable} resistance";
        }
    }
}
