using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_Armor : StatResolver
{
    public override float CalculateItemStat(Entity item)
    {
        var bodyMaterial = item.Body.Material;
        var density = bodyMaterial.Density;
        return density;
    }

    public override float CalculateActorStat(StatDef def, Actor actor)
        => actor.Gear.GetArmor();
}
