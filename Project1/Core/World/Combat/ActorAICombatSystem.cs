using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Systems;
using Project1.Core.World.Actions;
using System;

#nullable enable

namespace Project1.Core.World.Combat;

static class ActorAICombatSystem
{
    static internal (WorldActionDef actionDef, WorldActionData actionData) Decide(Actor actor, Actor target)
    {
        var world = (StaticWorld)actor.World;
        var session = world.Combat.Get(actor);
        ArgumentNullException.ThrowIfNull(session);
        if (session.IsUnstarted)
        {
            var result = SmartEquipSystem.EvaluateEquipmentChange(actor);
            if (result.ItemId != EntityRefId.Null)
                return (WorldActionDefOf.Equip, new WorldActionData_Equip(actor) { Decision = result });
        }
        return (WorldActionDefOf.Attack, new WorldActionWorker_Attack.Data(actor) { TargetId = target.RefId });
    }
}
