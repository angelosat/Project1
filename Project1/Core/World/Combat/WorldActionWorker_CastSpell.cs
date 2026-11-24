using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Services.Spells;
using Project1.Core.World.Actions;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_CastSpell : WorldActionWorker
{
    static WorldActionData_CastSpell Typed(WorldActionData actionData) => (WorldActionData_CastSpell)actionData;

    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_CastSpell)data;
        var actor = data.Actor;
        var target = data.World.Resolve<Actor>(typed.TargetId);
        var spell = typed.SpellDef;
        SpellSystem.SpellSuccess(actor, target, spell);
        //var cost = spell.ManaCost;
        //actor.Resources.ApplyDelta(ResourceDefOf.Mana, -cost);
        //foreach (var (key, mag) in spell.Effects)
        //    world.Events.Post(new ActorEffectAppliedNewEvent(world, target.RefId, key.Def, key.Target, mag));
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
        => $"Casting: {Typed(runtime.Data).SpellDef.LabelReadable}";
}
