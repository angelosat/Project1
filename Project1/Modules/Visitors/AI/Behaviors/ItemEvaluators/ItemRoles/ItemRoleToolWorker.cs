namespace Start_a_Town_
{
    class ItemRoleToolWorker : ItemRoleWorker
    {
        public override int GetEquippingScore(Actor actor, Entity item, ItemRoleDef context)
        {
            throw new System.NotImplementedException();
        }
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
        {
            var ability = item.ToolComponent?.Props?.ToolUse;
            if (ability is null)
                return -1;
            if (ability != role.Def)
                return -1;
            return (int)StatDefOf.ToolEffectiveness.GetValue(item);
        }
    }
}
