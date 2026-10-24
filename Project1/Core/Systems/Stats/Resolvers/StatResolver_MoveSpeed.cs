using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_MoveSpeed : StatResolver
{
    const float
        DensityMultiplier = .1f, 
        DensityReference = 20f, 
        DexterityMultiplier = .01f;

    public override float CalculateItemStat(Entity item)
    {
        var material = item.PrimaryMaterial;
        var densityMod = 1 + DensityMultiplier * (1 - material.Density / DensityReference);
        var craftsmanshipBonus = item.ToolComponent.Craftsmanship.Tier;
        var final = 100f * densityMod + craftsmanshipBonus;
        return final;
    }

    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        var fromGear = actor.Gear.GetMoveSpeed();
        if (fromGear == 0) 
            fromGear = 100;
        var fromDex = actor.Attributes.GetLevel(AttributeDefOf.Dexterity) * DexterityMultiplier;
        return fromGear * fromDex;
    }
}
