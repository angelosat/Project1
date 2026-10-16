namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatWalkSpeed : StatResolver
{
    public override float CalculateStat(Entity item)
        => (item.Quality.Rank - 2) * .2f;
        //=> 1f * item.Quality.Multiplier;
}
