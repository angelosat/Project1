using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public record IngredientGuiGroup
    {
        internal string Label;
        internal List<IngredientGuiEntry> Entries = [];
    }
    public record IngredientGuiEntry
    {
        internal string Label;
        internal List<IngredientGuiEntry> Children = [];
        internal Action Toggle;
        internal Func<bool> IsAllowed;
    }
    internal static class CraftingGuiBuilder
    {
        public static List<IngredientGuiGroup> Build(OrderSettings order)
        {
            var events = Ingame.Instance.Events;

            var list = new List<IngredientGuiGroup>();

            var rules = CraftingSystem.GetCraftingRules(order.ProductDef);
            foreach (var (bone, validRefinements, quantity) in rules)
            {
                var group = new IngredientGuiGroup() { Label = bone.Label };
                foreach (var refinement in validRefinements)
                {
                    var entry = new IngredientGuiEntry()
                    {
                        Label = $"{refinement.MaterialType.Label} {refinement.Label}",
                        Toggle = () => events.Post(new PlayerModifiedOrderFiltersEvent(order, bone, refinement, null)),
                        IsAllowed = () => order.IsAllowed(bone, refinement)
                    };
                    var mats = Def.GetDefs<MaterialDef>().Where(mat => mat.Type == refinement.MaterialType);
                    foreach (var mat in mats)
                        entry.Children.Add(new IngredientGuiEntry()
                        {
                            Label = mat.Label,
                            Toggle = () => events.Post(new PlayerModifiedOrderFiltersEvent(order, bone, refinement, mat)),
                            IsAllowed = () => order.IsAllowed(bone, mat)
                        });
                    group.Entries.Add(entry);
                }
                list.Add(group);
            }

            return list;
        }
        public static List<IngredientGuiGroup> Build(Stockpile stockpile)
        {
            var refinmentGroups = Def.GetDefs<MaterialRefinementDef>().GroupBy(r => Def.GetDefs<MaterialDef>().Where(m => m.Type == r.MaterialType));
            var events = Ingame.Instance.Events;
            List<(ItemDef item, Dictionary<Def, IEnumerable<MaterialDef>> mappings)> categories =
            [
                (ItemDefOf.Ingredient, Def.GetDefs<MaterialRefinementDef>().ToDictionary(r=>r as Def, r => Def.GetDefs<MaterialDef>().Where(m => m.Type == r.MaterialType))),
                (ItemDefOf.Tool, Def.GetDefs<ToolProfileDef>().ToDictionary(r=>r as Def, r=>Enumerable.Empty<MaterialDef>())),
                (ItemDefOf.Seeds, Def.GetDefs<PlantSpeciesDef>().ToDictionary(r=>r as Def, r=>Enumerable.Empty<MaterialDef>()))
            ];
            IngredientGuiGroup group = new();
            foreach (var (item, mappings) in categories)
            {
                var entry = new IngredientGuiEntry()
                {
                    Label = $"{item.Label}",
                    Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, null, null)),
                    IsAllowed = () => true
                };
                group.Entries.Add(entry);
                foreach (var (profile, materials) in mappings)
                {
                    var profileEntry = new IngredientGuiEntry()
                    {
                        Label = profile.Label,
                        Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, null)),
                        IsAllowed = () => true
                    };
                    entry.Children.Add(profileEntry);
                    foreach (var material in materials)
                    {
                        var materialEntry = new IngredientGuiEntry()
                        {
                            Label = material.Label,
                            Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, material)),
                            IsAllowed = () => true
                        };
                        profileEntry.Children.Add(materialEntry);
                    }
                }
            }
            return [group];
        }
        public static List<IngredientGuiGroup> BuildSmart(Stockpile stockpile)
        {
            var refinmentGroups = Def.GetDefs<MaterialRefinementDef>().GroupBy(r => Def.GetDefs<MaterialDef>().Where(m => m.Type == r.MaterialType));
            var events = Ingame.Instance.Events;
            List<(ItemDef item, IEnumerable<Def> profiles, IEnumerable<MaterialDef> materials)> categories =
            [
                (ItemDefOf.Ingredient, Def.GetDefs<MaterialRefinementDef>(), Def.GetDefs<MaterialDef>()),
                (ItemDefOf.Tool, Def.GetDefs<ToolProfileDef>(), Enumerable.Empty<MaterialDef>()),
                (ItemDefOf.Seeds, Def.GetDefs<PlantSpeciesDef>(), Enumerable.Empty<MaterialDef>()),
            ];
            IngredientGuiGroup group = new();
            foreach (var (item, profiles, materials) in categories)
            {
                var entry = new IngredientGuiEntry()
                {
                    Label = $"{item.Label}",
                    Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, null, null)),
                    IsAllowed = () => true
                };
                group.Entries.Add(entry);
                foreach (var profile in profiles)
                {
                    var profileEntry = new IngredientGuiEntry()
                    {
                        Label = profile.Label,
                        Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, null)),
                        IsAllowed = () => true
                    };
                    entry.Children.Add(profileEntry);
                    foreach(var material in materials)
                    {
                        var materialEntry = new IngredientGuiEntry()
                        {
                            Label = material.Label,
                            Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, material)),
                            IsAllowed = () => true
                        };
                        profileEntry.Children.Add(materialEntry);
                    }
                }
            }
            return [group];
        }
    }

    internal record struct PlayerModifiedOrderFiltersEvent(OrderSettings Order, BoneDef Bone, MaterialRefinementDef Refinement, MaterialDef Material) : IEventPayload { }
    internal record struct PlayerModifiedStockpileFiltersEvent(Stockpile Stockpile, ItemDef Item, Def Profile, MaterialDef Material) : IEventPayload { }
}
