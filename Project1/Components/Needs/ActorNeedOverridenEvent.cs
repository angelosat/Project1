namespace Start_a_Town_
{
    public record struct ActorNeedOverridenEvent(Actor Actor, NeedDef Need, float Percentage) : IEventPayload { }
}
