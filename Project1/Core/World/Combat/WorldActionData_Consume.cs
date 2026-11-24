using Project1.Core.Entities.Actors;
using Project1.Core.World.Actions;

namespace Project1.Core.World.Combat;

internal sealed record WorldActionData_Consume(Actor Actor) : WorldActionData(Actor)
{
    internal required EntityRefId ItemId;
}
