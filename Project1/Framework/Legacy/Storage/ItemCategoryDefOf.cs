using Project1.Core.Entities.Stats;
using Project1.Core.Base;

namespace Project1.Core.Legacy.Storage
{
    static class ItemCategoryDefOf
    {
        static public readonly ItemCategory Unlisted = new("Unlisted");
        static public readonly ItemCategory Equipment = new ItemCategory("Equipment").AddStats(StatDef.ToolStatPackage);
        static public readonly ItemCategory Wearables = new("Wearables");
        static public readonly ItemCategory RawMaterials = new("RawMaterials");
        static public readonly ItemCategory Manufactured = new("Manufactured");
        static public readonly ItemCategory FoodRaw = new("FoodRaw");
        static public readonly ItemCategory FoodCooked = new("FoodCooked");
        static public readonly ItemCategory Unfinished = new("Unfinished");

        static ItemCategoryDefOf()
        {
            Def.Register(typeof(ItemCategoryDefOf));
        }
    }
}
