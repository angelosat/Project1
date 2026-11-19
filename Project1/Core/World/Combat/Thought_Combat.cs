using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Thoughts;

namespace Project1.Core.World.Combat;

sealed class Thought_Combat : ThoughtSource<WorldCombatEvent>
{
    internal override void Handle(WorldCombatEvent e)
    {
        var world = e.World;
        var actorId = e.Actor;
        var actor = world.Resolve<Actor>(actorId);
        var combat = world.Combat.Get(actorId);
        var result = e.Result;
        var text = $"{result.Type} [{combat.GetOther(actor).Name}]";
        if (result.Damage.Value > 0)
            text += $" for {result.Damage.Value} {result.Damage.Type.LabelReadable} damage";
        actor.AI.State.Log.Write(text);
    }
}
