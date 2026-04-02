namespace Project1.Core.AI.Thought;

public abstract class ThoughtProcess
{
    public abstract void TickOnMap(AIState state);
    public abstract void TickOffMap(AIState state);
}
