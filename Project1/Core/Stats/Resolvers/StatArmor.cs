using Project1.Core.AI;
using Project1.Core.Entities.Actors;
using Project1.Core.Gear;
using System.Linq;

namespace Project1.Core.Entities.Stats.Resolvers;

sealed class StatArmor : StatResolver
{
    public override float CalculateStat(Entity obj)
    {
        var actor = obj as Actor;
        var gear = actor.GetGear();
        var value = gear.Sum(g => g.Def.ApparelProperties?.ArmorValue ?? 0);
        return value;
    }
}
