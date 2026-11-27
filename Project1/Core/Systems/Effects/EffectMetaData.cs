using Project1.Core.Helpers;
using Project1.Core.Systems.Magic;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Systems.Effects;

public readonly record struct EffectMetaData : ISerializableNewNew<EffectMetaData>
{
    public SpellSchoolDef School { get; init; }

    public static EffectMetaData Create(IDataReader r)
        => new()
        {
            School = r.ReadDef<SpellSchoolDef>()
        };

    public IDataWriter Write(IDataWriter w)
        => w.Write(School);
}
