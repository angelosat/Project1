using Project1.Core.Systems.Thoughts;

namespace Project1.Core.World.Combat;

sealed class Thought_Combat : ThoughtSource<WorldCombatEvent>
{
    internal override void Handle(WorldCombatEvent e)
    {
        var result = e.Result;
        var text = $"{result.Type} [{e.Combat.Other(e.Actor).Name}]";
        if (result.Damage.Value > 0)
            text += $" for {result.Damage.Value} {result.Damage.Type.LabelReadable} damage";
        e.Actor.AI.State.Log.Write(text);
    }
}
