using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using Project1.Core.Systems.Magic;
using Project1.Core.World.Actions;
using System;

#nullable enable

namespace Project1.Core.World.Combat;

static class ActorCombatAISystem
{
    static internal WorldActionData Decide(Actor actor, Actor target)
    {
        var world = (StaticWorld)actor.World;
        var session = world.Combat.Get(actor);
        ArgumentNullException.ThrowIfNull(session);

        if (actor.Biology.IsIncapacitated)
            throw new Exception();

        if (session.IsUnstarted)
        {
            var result = SmartEquipSystem.EvaluateEquipmentChange(actor);
            if (result.ItemId != EntityRefId.Null)
                return new WorldActionData_Equip(actor) { Decision = result };
        }

        if (IsInDanger(actor))
        {
            if (ConsumableSystem.CanUsePotion(actor) &&
               ConsumableSystem.FindInInventory(actor, ResourceDefOf.Health) is Entity item)
                return new WorldActionData_Consume(actor) { ItemId = item.RefId };

            if (MagicSystem.CanCastSpell(actor) &&
               MagicSystem.ConsiderSpell(actor, EffectDefOf.RestoreResource, ResourceDefOf.Health) is SpellDef spell)
                return new WorldActionData_CastSpell(actor) { SpellDef = spell, TargetId = actor.RefId };
        }

        if (session.CanFlee(actor))
        {
            var healthThreshold = .5f;
            var health = actor.Resources.Get(ResourceDefOf.Health);
            var healthPerc = health.Percentage;
            if (healthPerc < healthThreshold)
                return new WorldActionData_Default(actor, WorldActionDefOf.Flee);
        }

        return new WorldActionData_Attack(actor) { TargetId = target.RefId };
    }

    static internal bool IsConfident(Actor actor)
    {
        var health = actor.Resources.Get(ResourceDefOf.Health);
        return health.Percentage >= .9f;
    }

    static bool IsInDanger(Actor actor)
        => actor.Resources.Get(ResourceDefOf.Health).Percentage < .5f;
}
