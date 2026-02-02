using Start_a_Town_;

namespace Project1.Framework.Needs
{
    public record struct ActorNeedOverridenEvent(Actor Actor, NeedDef Need, float Percentage) : IEventPayload { }
}
