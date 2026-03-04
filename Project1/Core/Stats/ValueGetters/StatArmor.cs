using Project1.Core.Entities.Actors;
using System.Linq;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    sealed class StatArmor : StatWorker
    {
        public override float CalculateStat(Entity obj)
        {
            var actor = obj as Actor;
            var gear = actor.GetGear();
            var value = gear.Sum(g => g.Def.ApparelProperties?.ArmorValue ?? 0);
            return value;
        }
    }
}
