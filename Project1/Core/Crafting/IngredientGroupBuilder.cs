using Project1.Core.Entities;
using Project1.Core.Screens;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Plants;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Stockpiles;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Crafting
{
    internal static class IngredientGroupBuilder
    {
        public static List<IngredientGroup> Build(CraftingOrder order)
        {
            var events = Ingame.Instance.Events;

            var list = new List<IngredientGroup>();

            //var rules = CraftingSystem.GetCraftingRulesStruct(order.ProductDef);
            var rules = order.WorkstationCapability.Worker.GetCraftingRulesStruct(order.ProductDef);
            foreach (var rule in rules)
            {
                var bone = rule.Bone;
                var group = new IngredientGroup() { Label = bone.LabelReadable };
                foreach (var refinement in rule.MaterialTypes)
                {
                    var entry = new IngredientGroupEntry()
                    {
                        Label = $"{refinement.LabelReadable} {refinement.LabelReadable}",
                        Toggle = () => events.Post(new PlayerModifiedOrderFiltersEvent(order, bone, refinement, null)),
                        IsAllowed = () => order.IsAllowed(bone, refinement)
                    };
                    var mats = Def.Get<MaterialDef>().Where(mat => mat.Type == refinement);
                    foreach (var mat in mats)
                        entry.Children.Add(new IngredientGroupEntry()
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
        //public static List<IngredientGroup> Build(CraftingOrder order)
        //{
        //    var events = Ingame.Instance.Events;

        //    var list = new List<IngredientGroup>();

        //    var rules = CraftingSystem.GetCraftingRules(order.ProductDef);
        //    foreach (var (bone, validRefinements, quantity) in rules)
        //    {
        //        var group = new IngredientGroup() { Label = bone.LabelReadable };
        //        foreach (var refinement in validRefinements)
        //        {
        //            var entry = new IngredientGroupEntry()
        //            {
        //                Label = $"{refinement.MaterialType.LabelReadable} {refinement.LabelReadable}",
        //                Toggle = () => events.Post(new PlayerModifiedOrderFiltersEvent(order, bone, refinement, null)),
        //                IsAllowed = () => order.IsAllowed(bone, refinement)
        //            };
        //            var mats = Def.GetDefs<MaterialDef>().Where(mat => mat.Type == refinement.MaterialType);
        //            foreach (var mat in mats)
        //                entry.Children.Add(new IngredientGroupEntry()
        //                {
        //                    Label = mat.LabelReadable,
        //                    Toggle = () => events.Post(new PlayerModifiedOrderFiltersEvent(order, bone, refinement, mat)),
        //                    IsAllowed = () => order.IsAllowed(bone, mat)
        //                });
        //            group.Entries.Add(entry);
        //        }
        //        list.Add(group);
        //    }

        //    return list;
        //}
        public static List<IngredientGroup> Build(Stockpile stockpile)
        {
            var refinmentGroups = Def.Get<MaterialRefinementDef>().GroupBy(r => Def.Get<MaterialDef>().Where(m => m.Type == r.MaterialType));
            var events = Ingame.Instance.Events;
            List<(ItemDef item, Dictionary<Def, IEnumerable<MaterialDef>> mappings)> categories =
            [
                (ItemDefOf.Ingredient, Def.Get<MaterialRefinementDef>().ToDictionary(r=>r as Def, r => Def.Get<MaterialDef>().Where(m => m.Type == r.MaterialType))),
                (ItemDefOf.Tool, Def.Get<ToolProfileDef>().ToDictionary(r=>r as Def, r=>Enumerable.Empty<MaterialDef>())),
                (ItemDefOf.Seeds, Def.Get<PlantSpeciesDef>().ToDictionary(r=>r as Def, r=>Enumerable.Empty<MaterialDef>()))
            ];
            IngredientGroup group = new();
            foreach (var (item, mappings) in categories)
            {
                var entry = new IngredientGroupEntry()
                {
                    Label = $"{item.LabelReadable}",
                    Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, null, null)),
                    IsAllowed = () => stockpile.IsAllowed(item)
                };
                group.Entries.Add(entry);
                foreach (var (profile, materials) in mappings)
                {
                    var profileEntry = new IngredientGroupEntry()
                    {
                        Label = profile.LabelReadable,
                        Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, null)),
                        IsAllowed = () => stockpile.IsAllowed(item) && stockpile.IsAllowed(profile)
                    };
                    entry.Children.Add(profileEntry);
                    foreach (var material in materials)
                    {
                        var materialEntry = new IngredientGroupEntry()
                        {
                            Label = material.LabelReadable,
                            Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, material)),
                            IsAllowed = () => stockpile.IsAllowed(item) && stockpile.IsAllowed(profile, material)
                        };
                        profileEntry.Children.Add(materialEntry);
                    }
                }
            }
            return [group];
        }
        public static List<IngredientGroup> BuildSmart(Stockpile stockpile)
        {
            var refinmentGroups = Def.Get<MaterialRefinementDef>().GroupBy(r => Def.Get<MaterialDef>().Where(m => m.Type == r.MaterialType));
            var events = Ingame.Instance.Events;
            List<(ItemDef item, IEnumerable<Def> profiles, IEnumerable<MaterialDef> materials)> categories =
            [
                (ItemDefOf.Ingredient, Def.Get<MaterialRefinementDef>(), Def.Get<MaterialDef>()),
                (ItemDefOf.Tool, Def.Get<ToolProfileDef>(), Enumerable.Empty<MaterialDef>()),
                (ItemDefOf.Seeds, Def.Get<PlantSpeciesDef>(), Enumerable.Empty<MaterialDef>()),
            ];
            IngredientGroup group = new();
            foreach (var (item, profiles, materials) in categories)
            {
                var entry = new IngredientGroupEntry()
                {
                    Label = $"{item.LabelReadable}",
                    Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, null, null)),
                    IsAllowed = () => true
                };
                group.Entries.Add(entry);
                foreach (var profile in profiles)
                {
                    var profileEntry = new IngredientGroupEntry()
                    {
                        Label = profile.LabelReadable,
                        Toggle = () => events.Post(new PlayerModifiedStockpileFiltersEvent(stockpile, item, profile, null)),
                        IsAllowed = () => true
                    };
                    entry.Children.Add(profileEntry);
                    foreach (var material in materials)
                    {
                        var materialEntry = new IngredientGroupEntry()
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