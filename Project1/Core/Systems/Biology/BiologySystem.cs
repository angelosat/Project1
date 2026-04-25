using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Biology;

internal static class BiologySystem
{
    extension(Actor actor)
    {
        public BiologyComp Biology => actor.GetComponent<BiologyComp>();
    }
}
