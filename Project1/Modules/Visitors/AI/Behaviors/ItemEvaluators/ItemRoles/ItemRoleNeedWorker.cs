using Project1.Framework.Needs;
using System.Linq;

namespace Start_a_Town_
{
    class ItemRoleNeedWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            if (!item.TryGetComponent<ConsumableComponent>(out var consumableComp))
                return -100;
            var needDef = (NeedDef)role.Def;
            var needRestore = consumableComp.EffectsNew.Where(e => e.Target == needDef).Sum(e => e.Budget);
            if (needRestore <= 0)
                return -100;
            var need = actor.GetNeed(needDef);
            var needDeficit = need.Max - need.Value;// need.Deficit;
            return (int)(needRestore * needDeficit);
        }
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
        {
            if (!item.TryGetComponent<ConsumableComponent>(out var consumableComp))
                return -1;
            var nutrition = consumableComp.EffectsNew.Where(e => e.Target == role.Def).Sum(e => e.Budget);
            return nutrition * item.StackMax;
        }
    }
}