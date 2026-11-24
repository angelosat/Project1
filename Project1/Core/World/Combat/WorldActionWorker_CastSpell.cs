using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Services.Spells;
using Project1.Core.World.Actions;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.World.Combat;

internal sealed class WorldActionWorker_CastSpell : WorldActionWorker
{
    sealed record WorldActionData_CastSpell(Actor Actor) : WorldActionData(Actor)
    {
        internal required EntityRefId TargetId;

        internal required SpellDef SpellDef;

        public override ISerializable Read(IDataReader r)
        {
            this.TargetId = r.ReadEntityRefId();
            this.SpellDef = r.ReadDef<SpellDef>();
            return this;
        }

        public override void Write(IDataWriter w)
        => w.Write(this.TargetId)
            .Write(this.SpellDef);
    }

    internal override Type DataType => typeof(WorldActionData_CastSpell);

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

    public static (WorldActionDef def, WorldActionData data) Create(Actor actor, Actor target, SpellDef spell)
        => (WorldActionDefOf.CastSpell, new WorldActionData_CastSpell(actor)
        {
            TargetId = target.RefId,
            SpellDef = spell
        });
}
