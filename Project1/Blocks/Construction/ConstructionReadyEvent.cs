namespace Start_a_Town_
{
    public class ConstructionReadyEvent(BlockConstructionComp comp) : EventPayloadBase
    {
        public readonly BlockConstructionComp Source = comp;
    }
    public class ConstructionFinishedEvent(BlockConstructionComp comp) : EventPayloadBase
    {
        public readonly BlockConstructionComp Source = comp;
    }
    public class ConstructionUpdatedEvent(BlockConstructionComp comp) : EventPayloadBase
    {
        public readonly BlockConstructionComp Source = comp;
    }
}
