using Project1.Core.Helpers;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Effects;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Alchemy;

internal class AddOrderRequest_Alchemy : AddOrderRequest
{
    public EffectDef Effect { get; private set; }
    public Def Target { get; private set; }

    AddOrderRequest_Alchemy() : base(WorkstationCapabilityDefOf.Alchemy, ConsumableDefOf.Potion)
    {
        
    }

    public AddOrderRequest_Alchemy(EffectDef effect, Def target) 
        : base(WorkstationCapabilityDefOf.Alchemy, ConsumableDefOf.Potion)
    {
        this.Effect = effect;
        this.Target = target;
    }

    public override string GetLabel()
        => $"{ConsumableDefOf.Potion.Verb}: {this.Effect.Worker.Label(this.Target)}";

    protected override void WriteExtra(IDataWriter w)
    {
        w.Write(this.Effect);
        w.Write(this.Target);
    }
   
    protected override void ReadExtra(IDataReader r)
    {
        this.Effect = r.ReadDef<EffectDef>();
        this.Target = r.ReadDef();
    }

    protected override void SaveExtra(SaveTag tag)
    {
        tag.Save("Effect", this.Effect);
        tag.Save("Target", this.Target);
    }
    protected override void LoadExtra(SaveTag tag)
    {
        this.Effect = tag.LoadDef<EffectDef>("Effect");
        this.Target = tag.LoadDef("Target");
    }
}
