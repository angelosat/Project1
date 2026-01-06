namespace Start_a_Town_
{
    internal record struct PlantChoppedEvent(Actor Actor, TargetArgs Target, int Intensity) : IEventPayload { }
}