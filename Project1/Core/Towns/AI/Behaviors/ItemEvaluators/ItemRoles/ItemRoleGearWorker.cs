using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;

namespace Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles
{
    class ItemRoleGearWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context)
        {
            throw new System.NotImplementedException();
        }
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context)
        {
            var props = item.Def.ApparelProperties;
            if (props?.GearType != context.Def)
                return -1;
            return props.ArmorValue;
        }
    }
}
