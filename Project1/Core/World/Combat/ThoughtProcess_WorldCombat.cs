using Project1.Core.AI;
using Project1.Core.AI.Thought;
using Project1.Core.Simulation;
using Project1.Core.World.Actions;

namespace Project1.Core.World.Combat;

internal sealed class ThoughtProcess_InCombat : ThoughtProcess
{
    internal override void TickOffMap(AIState state)
    {
        var actor = state.Owner;
        var world = (StaticWorld)actor.World;

        var combat = world.Combat.Get(actor);
        if (combat is null)
            return;

        if (world.Actions.HasActiveAction(actor))
            return;

        if (!combat.CanAct(actor))
            return;

        // Decide which combat action to perform.
        //world.Actions.Begin(WorldActionDefOf.Attack, new WorldActionWorker_Attack.Data(actor) { TargetId = combat.GetOther(actor).RefId });
        //var action = WorldActionWorker_Attack.Create(actor, combat.GetOther(actor).RefId);
        world.Actions.Begin(new WorldActionData_Attack(actor) { TargetId = combat.GetOther(actor).RefId });
    }

    internal override void TickOnMap(AIState state)
    {
    }
}
