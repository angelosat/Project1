namespace Start_a_Town_
{
    class ItemRoleToolWorker : ItemRoleWorker
    {
        public override int GetSituationalScore(Actor actor, Entity item, ItemRoleDef role)
        {
            var task = actor.AI.State.Behavior?.Task;
            if (task is null)
                return -100;
            var target = task.EquipContextTarget;
            if (target is null)
                return 0;
            MaterialTypeDef targetMaterialType;
            if (target.Type == TargetType.Position)
                targetMaterialType = Block.GetBlockMaterial(target.Map, target.Global).Type;
            else if (target.Type == TargetType.Entity)
                targetMaterialType = target.Object.Body.Material.Type;
            else
                return 0;
            if (targetMaterialType == MaterialTypeDefOf.Wood && role.Def == ToolUseDefOf.Chopping ||
                   targetMaterialType == MaterialTypeDefOf.Soil && role.Def == ToolUseDefOf.Digging ||
                   targetMaterialType == MaterialTypeDefOf.Stone && role.Def == ToolUseDefOf.Mining)
                return 100;
            return 0;
        }
        public override int GetInventoryScore(Actor actor, Entity item, ItemRoleDef role)
        {
            var ability = item.ToolComponent?.ToolProperties?.ToolUse;
            if (ability is null)
                return -1;
            if (ability != role.Def)
                return -1;
            return (int)StatDefOf.ToolEffectiveness.GetValue(item);
        }
    }
}
