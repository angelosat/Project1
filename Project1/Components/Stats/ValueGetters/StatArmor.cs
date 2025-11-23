using System.Linq;

namespace Start_a_Town_
{
    class StatArmor : StatValueGetter
    {
        public StatArmor(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            var actor = obj as Actor;
            var gear = actor.GetGear();
            var value = gear.Sum(g => g.Def.ApparelProperties?.ArmorValue ?? 0);
            return value;
        }
    }
}
