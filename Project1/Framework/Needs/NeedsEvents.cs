using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;

namespace Project1.Framework.Needs
{
    public record struct ActorNeedOverridenEvent(Actor Actor, NeedDef Need, float Percentage) : IEventPayload { }
    record struct ActorNeedUpdatedEvent(Need Need) : IEventPayload { }

}
