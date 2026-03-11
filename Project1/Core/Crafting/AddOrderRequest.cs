#nullable enable

namespace Project1.Core.Crafting
{
    public record AddOrderRequest(WorkstationCapabilityDef WorkstationCapability, Def? ProductDef) 
    {
        //public string GetLabel() => this.ProductDef?.LabelReadable ?? this.WorkstationCapability.LabelReadable;"
        public string GetLabel()
            => this.ProductDef is Def def ? 
            $"{this.WorkstationCapability.LabelReadable}: {def.LabelReadable}" :
            $"{this.WorkstationCapability.LabelReadable}";
    }
}
