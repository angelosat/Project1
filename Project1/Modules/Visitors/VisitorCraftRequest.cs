using System.Collections.Generic;

namespace Start_a_Town_
{
    public class VisitorCraftRequest
    {
        public readonly CraftOrder Order;
        readonly Dictionary<string, ItemMaterialAmount> Preferences = new();

        public VisitorCraftRequest(CraftOrder order, IEnumerable<(string reagentName, ItemDef item, MaterialDef material)> preferences)
        {
            this.Order = order;
            foreach (var (reagentName, item, material) in preferences)
                this.Preferences.Add(reagentName, new ItemMaterialAmount(item, material, 1));
        }
        public (ItemDef item, MaterialDef material) GetPreference(string reagentName)
        {
            var i = this.Preferences[reagentName];
            return (i.Item, i.Material);
        }

        public IEnumerable<(string reagentName, ItemDef item, MaterialDef material)> GetPreferences()
        {
            foreach (var i in this.Preferences)
                yield return (i.Key, i.Value.Item, i.Value.Material);
        }
    }
}
