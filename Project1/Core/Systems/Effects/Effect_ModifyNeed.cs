using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Needs;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Effects;

public sealed record EffectConfig_ModifyNeed(NeedDef Need, float Delta) : EffectConfig
{
    public override EffectDef Def => EffectDefOf.ModifyNeed;

    public override EffectData CreateData()
        => new EffectData_ModifyNeed() { Need = Need, Delta = Delta };

    public override string GetString(float scaling)
        => $"Modifies {this.Need.LabelReadable} by {this.Delta}";
}

public sealed class EffectData_ModifyNeed : EffectData
{
    public override EffectDef Def => EffectDefOf.ModifyNeed;

    public required NeedDef Need;

    public required float Delta;

    public override ISerializable Read(IDataReader r)
    {
        this.Need = r.ReadDef<NeedDef>();
        this.Delta = r.ReadSingle();
        return this;
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.Need);
        w.Write(this.Delta);
    }
}

internal sealed class Effect_ModifyNeed : EntityEffectWorker<EffectData_ModifyNeed>
{
    protected override void OnPerform(Actor actor, EffectData_ModifyNeed dto)
    {
        var need = actor.Needs.NeedsNew[dto.Need];
        need.ApplyAccumulatorDelta(dto.Delta);
    }
    //protected override void OnStart(Actor actor, EntityEffectRuntime wrapper)
    //{
    //}
    //protected override void OnTick(Actor actor, EntityEffectRuntime wrapper)
    //{
    //    var need = actor.Needs.NeedsNew[(NeedDef)wrapper.Target];
    //    need.ApplyAccumulatorDelta(1f / wrapper.TicksPerUnit);
    //}
    //protected override void OnFinish(Actor actor, EntityEffectRuntime wrapper)
    //{
    //}
}
