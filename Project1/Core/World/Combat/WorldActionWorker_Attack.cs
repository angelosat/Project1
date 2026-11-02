using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Stats;
using Project1.Core.World.Actions;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Combat;

internal class WorldActionWorker_Attack : WorldActionWorker
{
    internal sealed record Data(FrontierRuntime Frontier, Actor Actor) : WorldActionData(Frontier, Actor)
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

    //internal override Data CreateData(FrontierRuntime frontier, Actor actor)
    //    => new(frontier, actor);
    static Data Typed(WorldActionData data) => (Data)data;

    internal override void Resolve(WorldActionData data)
    {
        CombatSystem.PerformAttack(data.Actor);
    }

    internal override string GetStatusText(WorldActionRuntime runtime)
    {
        return $"{base.GetStatusText(runtime)}: {Typed(runtime.Data).Target.Name}";
    }

    internal override float GetDurationMultiplier(WorldActionData data)
    {
        var actor = data.Actor;
        var speed = actor.Stats.Get(StatDefOf.SwingSpeed);
        return speed;
    }
}

