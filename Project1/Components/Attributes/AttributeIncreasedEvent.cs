namespace Start_a_Town_
{
    internal class AttributeIncreasedEvent(Actor owner, AttributeDef def, float delta) : IEventPayload
    {
        public readonly Actor Owner = owner;
        public readonly AttributeDef Def = def;
        public readonly float Delta = delta;
    }
    internal class ResourceAdjustedEvent(Entity owner, ResourceDef def, float delta) : IEventPayload
    {
        public readonly Entity Owner = owner;
        public readonly ResourceDef Def = def;
        public readonly float Delta = delta;
    }
}