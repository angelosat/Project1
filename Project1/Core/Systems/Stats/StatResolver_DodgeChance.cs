using Project1.Core.Attributes;
using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Entities.Stats.Resolvers;

namespace Project1.Core.Stats;

sealed class StatResolver_DodgeChance : StatResolver
{
    const int _dMin = 5, _dMax = 40;

    public override float CalculateActorStat(StatDef def, Actor actor)
    {
        var dex = actor.Attributes.GetLevel(AttributeDefOf.Dexterity);
        var d = _dMin + _dMax * dex / (dex + 100f);
        return d;
    }
}
