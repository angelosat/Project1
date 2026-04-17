#nullable enable

using Project1.Core.Blocks;
using System.Collections.Generic;

namespace Project1.Core.Crafting;

public record AddOrderRequest(WorkstationCapabilityDef WorkstationCapability, Def? ProductDef) 
{
    readonly List<CraftingOrderAvailabilityCondition> _conditions = [];
    internal OrderAvailabilityResult IsAvailable(BlockWorkstationComp comp)
    {
        List<string> messages = [];
        bool result = true;
        foreach (var cond in this._conditions)
        {
            if (!cond.Predicate(comp))
            {
                result = false;
                messages.Add($"{cond.Label}");
            }
        }
        return new() { Result = result, Message = string.Join("\n", messages) };
    }
    public virtual string GetLabel()
        => this.ProductDef is Def def ? 
        $"{this.WorkstationCapability.LabelReadable}: {def.LabelReadable}" :
        $"{this.WorkstationCapability.LabelReadable}";

    internal AddOrderRequest AddCondition(CraftingOrderAvailabilityCondition condition)
    {
        this._conditions.Add(condition);
        return this;
    }
}
