using Project1.Core.Entities.Actors;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatResolver_Armor : StatResolver
{
    public override float CalculateStat(Entity item)
    {
        var bodyMaterial = item.Body.Material;
        var density = bodyMaterial.Density;
        return density;

        //var actor = item as Actor;
        //var gear = actor.GetGear();
        //var value = gear.Sum(g => g.Def.ApparelProperties?.ArmorValue ?? 0);
        //return value;
    }
    public override float CalculateStatActor(StatDef def, Actor actor)
        => actor.Gear.GetArmor();
}
