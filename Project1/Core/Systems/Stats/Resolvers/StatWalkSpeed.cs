using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers
{
    sealed class StatWalkSpeed : StatResolver
    {
        public override float CalculateStat(Entity item)
            => 1f * item.Quality.Multiplier;
    }
}
