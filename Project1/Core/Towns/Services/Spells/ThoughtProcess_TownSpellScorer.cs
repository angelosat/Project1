using Project1.Core.AI;
using Project1.Core.AI.Thought;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Magic;
using System.Linq;

namespace Project1.Core.Towns.Services.Spells;

internal class ThoughtProcess_TownSpellScorer : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
    }

    internal override void TickOnMap(AIState state)
    {
        var actor = state.Owner;
        var map = actor.Map;
        var town = map.Town;
        var manager = town.Spells;
        var availableSpells = manager.GetAvailableSpells();
        var scored = availableSpells.Select(s => (s, Score(actor, s.Spell))).OrderByDescending(s => s.Item2);
        foreach (var spell in scored)
        {
            // TODO check any additional availability conditions and set it as the desired spell and return
        }
    }

    static int Score(Actor customer, SpellDef spell)
    {
        var total = 0;
        foreach (var (effect, target) in spell.Effects)
        {
            total += SpellSystem.Score(customer, effect, target);
        }
        return total;
    }
}
