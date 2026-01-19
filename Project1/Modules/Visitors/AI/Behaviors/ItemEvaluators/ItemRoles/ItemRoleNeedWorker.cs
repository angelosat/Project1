using Start_a_Town_.Components;
using System.Linq;

namespace Start_a_Town_
{
    class ItemRoleNeedWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            if (!item.TryGetComponent<ConsumableComponent>(out var consumableComp))
                return -100;
            var nutrition = consumableComp.EffectsNew.Where(e => e.Target == role.Def).Sum(e => e.Budget);
            if (nutrition <= 0)
                return -100;
            var hungerDeficit = actor.GetNeed(NeedDefOf.Hunger).Deficit;
            return (int)(nutrition * hungerDeficit);
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