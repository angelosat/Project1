using Project1.Core.Entities;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;
using System.Collections.Generic;

namespace Project1.Core.Towns
{
    public class VisitorCraftRequest
    {
        public readonly CraftOrderOld Order;
        readonly Dictionary<string, ItemMaterialAmount> Preferences = new();

        public VisitorCraftRequest(CraftOrderOld order, IEnumerable<(string reagentName, ItemDef item, MaterialDef material)> preferences)
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
