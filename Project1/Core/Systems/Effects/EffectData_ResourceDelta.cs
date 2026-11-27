using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Effects;

public sealed record EffectConfig_ResourceDelta(ResourceDef Resource, int Delta) : EffectConfig
{
    public override EffectData CreateData()
    {
        return new EffectData_ResourceDelta() { Delta = Delta , Resource = Resource};
    }

    public override string GetString(float scaling)
    {
        throw new System.NotImplementedException();
    }
}

public sealed class EffectData_ResourceDelta : EffectData<EffectConfig_ResourceDelta>
{
    public override EffectDef Def => EffectDefOf.ResourceDelta;

    public required ResourceDef Resource;

    public required int Delta;

    public override void Write(IDataWriter w)
        => w.Write(Resource)
            .Write(Delta);

    public override ISerializable Read(IDataReader r)
    {
        Resource = r.ReadDef<ResourceDef>();
        Delta = r.ReadInt32();
        return this;
    }
}
