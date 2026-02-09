using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Towns
{
    internal record struct JobUpdatedEvent(Actor Actor, JobDef Job) : IEventPayload { }

}
