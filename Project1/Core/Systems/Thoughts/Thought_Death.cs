using Project1.Core.Entities;

namespace Project1.Core.Systems.Thoughts;

internal class Thought_Death : ThoughtSource<ActorDeathEvent>
{
    internal override void Handle(ActorDeathEvent e)
    {
        e.Actor.AI.State.Log.Write("Died");
    }
}
