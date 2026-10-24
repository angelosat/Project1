using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_MoveSpeed : StatResolver
{
    public override float CalculateStat(Entity item)
        => 1f;// (item.Quality.Rank - QualityDefOf.Common.Rank) * .2f;
              //=> 1f * item.Quality.Multiplier;

    public override float CalculateStatActor(StatDef def, Actor actor)
        => actor.Attributes.GetLevel(AttributeDefOf.Dexterity);
}
