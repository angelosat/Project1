using Project1.Core.Base;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Needs
{
    public record struct ActorNeedOverridenEvent(Actor Actor, NeedDef Need, float Percentage) : IEventPayload { }
    record struct ActorNeedUpdatedEvent(Need Need) : IEventPayload { }

}
