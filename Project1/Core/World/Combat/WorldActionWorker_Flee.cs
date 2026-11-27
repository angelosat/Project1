using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;
using System;

namespace Project1.Core.World.Combat;

internal sealed record WorldActionData_Flee(Actor Actor) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => WorldActionDefOf.Flee;
}


internal sealed class WorldActionWorker_Flee : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData);

    internal override void Resolve(WorldActionData data)
    {
        CombatSystem.AttemptFlee(data.Actor);
    }
}
