namespace Start_a_Town_
{
    public class ConstructionReadyEvent(BlockConstructionComp comp/*, bool ready*/) : EventPayloadBase
    {
        public readonly BlockConstructionComp Comp = comp;
        //public readonly bool Ready = ready;
    }
}
