using Project1.Core.Crafting;
using Project1.Core.Effects;
using Project1.Core.Systems.Consumables;

namespace Project1.Core.Systems.Alchemy;

internal record AddOrderRequest_Alchemy : AddOrderRequest
{
    readonly EffectDef Effect;
    readonly Def Target;
    public AddOrderRequest_Alchemy(WorkstationCapabilityDef WorkstationCapability, EffectDef effect, Def target) 
        : base(WorkstationCapability, ConsumableDefOf.Potion)
    {
        this.Effect = effect;
        this.Target = target;
    }

    public override string GetLabel()
        => $"{ConsumableDefOf.Potion.Verb}: {this.Effect.Worker.Label(this.Target)}";
}
