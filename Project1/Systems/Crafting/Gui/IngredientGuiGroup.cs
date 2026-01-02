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
    }

    internal sealed record PlayerModifiedOrderFiltersEvent(OrderSettings Order, BoneDef Bone, MaterialRefinementDef Refinement, MaterialDef Material) : IEventPayload { }
}
