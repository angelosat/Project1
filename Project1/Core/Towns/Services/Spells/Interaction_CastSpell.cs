using Project1.Core.Interactions;
using Project1.Core.Systems.Magic;

namespace Project1.Core.Towns.Services.Spells;

sealed class Interaction_CastSpell : InteractionLogic
{
    static SpellDef Spell(InteractionContext ctx) => ctx.Actor.CurrentPlan.Spell;

    internal override void OnStart(Interaction i)
    {
        var spell = Spell(i.Context);
        i.Progress.SetMax((int)Ticks.FromSeconds(spell.CastTimeSeconds));
    }

    internal override bool HasSucceeded(Interaction i)
        => i.Progress.IsFinished;

    internal override void OnFinish(Interaction i)
    {
        var spell = Spell(i.Context);
        //spell.Worker.Cast(i.Actor, i.Target);
        i.Actor.Cast(spell, i.Target);
        i.Actor.Map.Events.Post(new SpellCastEvent(i.Actor, i.Target, spell));
    }
}
