using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.AI.Thought;

internal sealed class ThoughtProcess_ChangeArea : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
        throw new System.NotImplementedException();
    }

    internal override void TickOnMap(AIState state)
    {
    }

}
public abstract class ThoughtProcess
{
    internal abstract void TickOnMap(AIState state);
    internal abstract void TickOffMap(AIState state);
    internal virtual int GetFrontierScore(Actor actor, FrontierDef frontier) => 0;
}
