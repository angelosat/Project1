namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatWalkSpeed : StatResolver
{
    public override float CalculateStat(Entity item)
        => (item.Quality.Rank - QualityDefOf.Common.Rank) * .2f;
        //=> 1f * item.Quality.Multiplier;
}
