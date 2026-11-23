using Project1.Core.World.Actions;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_Flee : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        CombatSystem.AttemptFlee(data.Actor);
    }
}
