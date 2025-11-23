namespace Start_a_Town_
{
    class ItemRoleTool : ItemRole
    {
        public JobDef ToolAbility;
        public override IItemPreferenceContext Context => this.ToolAbility;

        public ItemRoleTool(JobDef ttype)
        {
            this.ToolAbility = ttype;
        }

        public override int Score(Actor actor, Entity item)
        {
            var ability = item.ToolComponent?.Props?.ToolUse;
            if (ability is null)
                return -1;
            if (ability != this.ToolAbility.ToolUse)
                return -1;
            return (int)StatDefOf.ToolEffectiveness.GetValue(item);
        }
        public override string ToString()
        {
            return $"{this.ToolAbility.Label}";
        }
    }
}
