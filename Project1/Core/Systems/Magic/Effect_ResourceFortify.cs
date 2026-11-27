using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Effects;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Magic;

public sealed record EffectConfig_ResourceFortify : EffectConfig
{
    public override EffectDef Def => EffectDefOf.ResourceFortify;

    public required ResourceDef Resource { get; init; }

    public required int Delta { get; init; }

    public override EffectData CreateData()
        => new EffectData_ResourceFortify() { Delta = Delta, Resource = Resource };

    public override string GetString(float scaling)
    {
        throw new System.NotImplementedException();
    }
}

public sealed class EffectData_ResourceFortify : EffectData<EffectConfig_ResourceFortify>
{
    public override EffectDef Def => EffectDefOf.ResourceFortify;

    public required ResourceDef Resource;

    public required int Delta;

    public override ISerializable Read(IDataReader r)
    {
        this.Resource = r.ReadDef<ResourceDef>();
        this.Delta = r.ReadInt32();
        return this;
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.Resource);
        w.Write(this.Delta);
    }
}

public sealed class Effect_ResourceFortify : EntityEffectWorker<EffectData_ResourceFortify>
{
    internal override Color Tint(EffectData_ResourceFortify dto) => dto.Resource.Color;

    protected override void OnStart(Actor actor, EffectData_ResourceFortify dto)
    {
        var max = actor.Resources.GetMax(dto.Resource);
        actor.Resources.SetMax(dto.Resource, max + dto.Delta);
    }

    protected override void OnFinish(Actor actor, EffectData_ResourceFortify dto)
    {
        var max = actor.Resources.GetMax(dto.Resource);
        actor.Resources.SetMax(dto.Resource, max - dto.Delta);
    }

    //protected override void OnStart(Actor actor, EntityEffectRuntime wrapper)
    //{
    //    var resource = (ResourceDef)wrapper.Target;
    //    var max = actor.Resources.GetMax(resource);
    //    actor.Resources.SetMax(resource, max + (int)wrapper.RemainingBudget.Value);
    //}
    //protected override void OnFinish(Actor actor, EntityEffectRuntime wrapper)
    //{
    //    var resource = (ResourceDef)wrapper.Target;
    //    var max = actor.Resources.GetMax(resource);
    //    actor.Resources.SetMax(resource, max - (int)wrapper.RemainingBudget.Value);
    //}

    //internal override Color GetTint(Def target)
    //    => ((ResourceDef)target).Color;
}
