using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;
using Project1.Core.Systems.Tools;

namespace Project1.Core.Systems.Stats.Resolvers;

sealed class StatResolver_Damage : StatResolver
{
    public override string GetName(StatDef def, Entity item)
    {
        var profile = item.Profile as GearProfileDef;
        return $"{profile.Damage.LabelReadable} {def.LabelReadable}";
    }

    public override float CalculateActorStat(StatDef def, Actor actor)
    => actor.Gear.GetDamage();
}
