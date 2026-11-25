using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Stats;
using Project1.Core.Systems.Biology;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;
using System;
using System.Linq;

namespace Project1.Core.World.Combat;

internal sealed record WorldActionData_Attack(Actor Actor) : WorldActionData(Actor)
{
    internal override WorldActionDef Def => WorldActionDefOf.Attack;

    internal required EntityRefId TargetId;
    internal Actor Target => field ??= this.Actor.World.Resolve<Actor>(this.TargetId);
    public override void Write(IDataWriter w)
    {
        w.Write(this.TargetId);
    }
    public override ISerializable Read(IDataReader r)
    {
        this.TargetId = r.ReadEntityRefId();
        return this;
    }
}

internal sealed class WorldActionWorker_Attack : WorldActionWorker
{
    internal override Type DataType => typeof(WorldActionData_Attack);

    //public static (WorldActionDef, WorldActionData) Create(Actor actor, EntityRefId targetId)
    //    => (WorldActionDefOf.Attack, new WorldActionData_Attack(actor) { TargetId =  targetId });

    static WorldActionData_Attack Typed(WorldActionData data) => (WorldActionData_Attack)data;

    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_Attack)data;
        var actor = typed.Actor;

        // if this is the first attack on an ambushed opponent, this means that there is not compat runtime yet
        // so interuprt the target actor's worldaction if any, and create a combat runtime
        var target = typed.Target;
        var world = data.World;
        var frontiers = (FrontierManager)world.Space;

        // check if the target is still in the relevant overworld context and cancel the combat if not
        if (actor.Frontier != target.Frontier)
        {
            world.Combat.Cancel(actor);
            return;
        }

        if (frontiers.GetPoi(actor) != frontiers.GetPoi(target))
        {
            world.Combat.Cancel(actor);
            return;
        }

        CombatSystem.ResolveWeaponAttack(actor, target);
        if (target.Biology.IsIncapacitated)
        {
            var invItems = target.Inventory.GetItems();
            var gear = target.Gear.GetItems();
            var loot = invItems.Union(gear).Select(i => (EntityRefId)i.RefId).ToArray();
            WorldActionWorker_Loot.Begin(actor, loot);
        }
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
    {
        return $"{base.GetStatusText(runtime)}: {Typed(runtime.Data).Target.Name}";
    }

    internal override float GetDurationMultiplier(WorldActionData data)
    {
        var actor = data.Actor;
        var dexterity = actor.Attributes.Get(AttributeDefOf.Dexterity);
        var dexMultiplier = 1 - dexterity / 100f;
        var speed = actor.Stats.GetFinal(StatDefOf.SwingSpeed);
        return speed * dexMultiplier;
    }
}

