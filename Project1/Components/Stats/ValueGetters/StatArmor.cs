using System.Linq;

namespace Start_a_Town_
{
    class StatArmor : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var actor = obj as Actor;
            var gear = actor.GetGear();
            var value = gear.Sum(g => g.Def.ApparelProperties?.ArmorValue ?? 0);
            return value;
        }
    }
}
