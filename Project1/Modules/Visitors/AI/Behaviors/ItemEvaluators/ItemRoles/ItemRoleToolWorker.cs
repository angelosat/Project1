namespace Start_a_Town_
{
    class ItemRoleToolWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            var task = actor.CurrentTask;
            var target = task.EquipContextTarget;
            if (target is null)
                return 0;
            var targetMaterial = Block.GetBlockMaterial(target.Map, target.Global);
            if (targetMaterial.Type == MaterialTypeDefOf.Wood && role.Def == ToolUseDefOf.Chopping ||
                targetMaterial.Type == MaterialTypeDefOf.Soil && role.Def == ToolUseDefOf.Digging ||
                targetMaterial.Type == MaterialTypeDefOf.Stone && role.Def == ToolUseDefOf.Mining)
                return 100;
            return 0;
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
