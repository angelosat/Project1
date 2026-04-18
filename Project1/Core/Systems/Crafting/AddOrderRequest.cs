#nullable enable

using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System.Collections.Generic;

namespace Project1.Core.Systems.Crafting;

public class AddOrderRequest : ISaveableNewNew<AddOrderRequest>, ISerializableNewNew<AddOrderRequest>
{
    public AddOrderRequest(WorkstationCapabilityDef workstationCapability, Def? productDef)
    {
        WorkstationCapability = workstationCapability;
        ProductDef = productDef;
    }

    AddOrderRequest()
    {
        
    }
    internal WorkstationCapabilityDef WorkstationCapability;
    internal Def? ProductDef;

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

    public IDataWriter Write(IDataWriter w)
    {
        w.Write(this.WorkstationCapability);
        w.Write(this.ProductDef);
        this.WriteExtra(w);
        return w;
    }
    protected virtual void WriteExtra(IDataWriter w) { }
    public static AddOrderRequest Create(IDataReader r)
    {
        var cap = r.ReadDef<WorkstationCapabilityDef>();
        var req = ActivatorSafe<AddOrderRequest>.CreateInstance(cap.Worker.OrderRequestType);
        req.WorkstationCapability = cap;
        req.ProductDef = r.ReadDef();
        req.ReadExtra(r);
        return req;
    }
    public IDataReader Read(IDataReader r)
    {
        this.WorkstationCapability = r.ReadDef<WorkstationCapabilityDef>();
        this.ProductDef = r.ReadDef();
        this.ReadExtra(r);
        return r;
    }
    protected virtual void ReadExtra(IDataReader r) { }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Capability", this.WorkstationCapability);
        tag.Save("Product", this.ProductDef);
        this.SaveExtra(tag);
        return tag;
    }
    protected virtual void SaveExtra(SaveTag tag) { }
    public static AddOrderRequest Create(SaveTag tag)
    {
        var capability = tag.LoadDef<WorkstationCapabilityDef>("Capability");
        var req = ActivatorSafe<AddOrderRequest>.CreateInstance(capability.Worker.OrderRequestType);
        req.WorkstationCapability = capability;
        req.ProductDef = tag.LoadDef("Product");
        req.LoadExtra(tag);
        return req;
    }
    protected virtual void LoadExtra(SaveTag tag) { }

    //IDataWriter ISerializableNewNew<AddOrderRequest>.Write(IDataWriter w)
    //{
    //    return Write(w);
    //}

    //static AddOrderRequest ISerializableNewNew<AddOrderRequest>.Create(IDataReader r)
    //{
    //    return this.Create(r);
    //}
    
}
