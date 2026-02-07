using Project1.Core.Entities;

namespace Project1.Core.Legacy.Crafting.ReagentFilters
{
    public class ReagentFilterItem
    {
        public ItemDef Specific;

        public ReagentFilterItem()
        {
        }

        public ReagentFilterItem(ItemDef itemDef)
        {
            this.Specific = itemDef;
        }

        public bool Condition(ItemDef def)
        {
            return this.Specific == null || def == this.Specific;
        }
    }
}