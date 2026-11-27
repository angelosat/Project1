using Project1.Core.Helpers;
using Project1.Core.Systems.Effects;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Systems.Consumables;

public sealed record EffectConfig_ConsumableCooldown(ConsumableDef Family) : EffectConfig
{
    public override EffectData CreateData()
        => new EffectData_ConsumableCooldown() { Family = Family };

    public override string GetString(float scaling)
        => $"{this.Family.LabelReadable} lockout";
}

public sealed class EffectData_ConsumableCooldown : EffectData<EffectConfig_ConsumableCooldown>
{
    public override EffectDef Def => EffectDefOf.ConsumableCooldown;

    public required ConsumableDef Family;

    public override ISerializable Read(IDataReader r)
    {
        this.Family = r.ReadDef<ConsumableDef>();
        return this;
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.Family);
    }
}

internal sealed class Effect_ConsumableCooldown : EntityEffectWorker<EffectData_ConsumableCooldown>
{
    protected override string GetString(EffectData_ConsumableCooldown dto)
        => dto.Family.LabelReadable;
    //internal override string Label(Def target)
    //    => EffectDefOf.ConsumableCooldown.LabelReadable;

    //internal override string GetString(Def? target, int magnitude)
    //    //=> $"Prevent potion consumption for {magnitude} m";
    //    => $"{target.LabelReadable} lockout";
    internal override string Description(EffectData_ConsumableCooldown dto) => 
        $"{dto.Family.LabelReadable} lockout";
}
