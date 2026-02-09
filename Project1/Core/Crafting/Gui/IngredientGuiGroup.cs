using Project1.Core.Entities;
using Project1.Core.Plants;
using Project1.Core.Materials;
using Project1.Core.Screens;
using Project1.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Crafting;

namespace Project1.Core.Crafting.Gui
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
                var group = new IngredientGuiGroup() { Label = bone.LabelReadable };
                foreach (var refinement in validRefinements)
                {
                    var entry = new IngredientGuiEntry()
                    {
                        Label = $"{refinement.MaterialType.LabelReadable} {refinement.LabelReadable}",
                        Toggle = () => events.Post(new PlayerModifiedOrderFiltersEvent(order, bone, refinement, null)),
                        IsAllowed = () => order.IsAllowed(bone, refinement)
                    };
                    var mats = Def.GetDefs<MaterialDef>().Where(mat => mat.Type == refinement.MaterialType);
                    foreach (var mat in mats)
                        entry.Children.Add(new IngredientGuiEntry()
                        {
                            Label = mat.LabelReadable,
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
                    Label = $"{item.LabelReadable}",
                    Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, null, null)),
                    IsAllowed = () => stockpile.IsAllowed(item)
                };
                group.Entries.Add(entry);
                foreach (var (profile, materials) in mappings)
                {
                    var profileEntry = new IngredientGuiEntry()
                    {
                        Label = profile.LabelReadable,
                        Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, null)),
                        IsAllowed = () => stockpile.IsAllowed(item) && stockpile.IsAllowed(profile)
                    };
                    entry.Children.Add(profileEntry);
                    foreach (var material in materials)
                    {
                        var materialEntry = new IngredientGuiEntry()
                        {
                            Label = material.LabelReadable,
                            Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, material)),
                            IsAllowed = () => stockpile.IsAllowed(item) && stockpile.IsAllowed(profile, material)//!stockpile.FiltersNew.Contains((profile, material))
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
                    Label = $"{item.LabelReadable}",
                    Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, null, null)),
                    IsAllowed = () => true
                };
                group.Entries.Add(entry);
                foreach (var profile in profiles)
                {
                    var profileEntry = new IngredientGuiEntry()
                    {
                        Label = profile.LabelReadable,
                        Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, null)),
                        IsAllowed = () => true
                    };
                    entry.Children.Add(profileEntry);
                    foreach (var material in materials)
                    {
                        var materialEntry = new IngredientGuiEntry()
                        {
                            Label = material.LabelReadable,
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

}
