namespace Start_a_Town_
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
