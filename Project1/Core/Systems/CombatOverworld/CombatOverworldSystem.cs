using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.CombatOverworld
{
    internal class CombatOverworldSystem
    {
    }

    internal class CombatOverworldRuntime(Actor first, Actor second)
    {
        Actor First = first, Second = second;
    }
}
