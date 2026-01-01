namespace Start_a_Town_
{
    public class ActorNeedOverridenEvent(Actor actor, NeedDef need, float value) : IEventPayload
    {
        public Actor Actor = actor;
        public readonly NeedDef Need = need;
        public readonly float Percentage = value;
    }
}
