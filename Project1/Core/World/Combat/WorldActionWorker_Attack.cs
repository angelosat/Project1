using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Stats;
using Project1.Core.Systems.Biology;
using Project1.Core.World.Actions;
using Project1.Framework.Serialization;
using System.Linq;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_Attack : WorldActionWorker
{
    internal sealed record Data(Actor Actor) : WorldActionData(Actor)
    {
        internal required EntityRefId TargetId;
        internal Actor Target => field ??= this.Actor.World.Get<Actor>(this.TargetId);
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

    static Data Typed(WorldActionData data) => (Data)data;

    internal override void Resolve(WorldActionData data)
    {
        var typed = (Data)data;
        var actor = typed.Actor;

        // if this is the first attack on an ambushed opponent, this means that there is not compat runtime yet
        // so interuprt the target actor's worldaction if any, and create a combat runtime
        var target = typed.Target;

        CombatSystem.PerformAttack(actor, target);
        var world = data.World;
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
        var speed = actor.Stats.Get(StatDefOf.SwingSpeed);
        return speed * dexMultiplier;
    }
}

