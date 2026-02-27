using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Duties;
using Project1.Framework.Events;

namespace Project1.Core.Towns
{
    internal record struct MemberAddedEvent(Actor Actor) : IEventPayload { }
    internal record struct MemberRemovedEvent(Actor Actor) : IEventPayload { }

}
