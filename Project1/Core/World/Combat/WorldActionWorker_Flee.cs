using Project1.Core.World.Actions;
using System;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_Flee : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData);

    internal override void Resolve(WorldActionData data)
    {
        CombatSystem.AttemptFlee(data.Actor);
    }
}
