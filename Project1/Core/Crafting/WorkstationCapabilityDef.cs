using Project1.Core.AI;
using Project1.Core.Blocks;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Crafting
{
    public class WorkstationCapabilityDef(string name, Type workerType) : Def(name)
    {
        public Type Output;
        public Def[] OutputSpecific = [];
        public PlanDef Plan;
        public WorkstationCapabilityWorker Worker = ActivatorSafe<WorkstationCapabilityWorker>.CreateInstance(workerType);

    }
    public abstract class WorkstationCapabilityWorker
    {
        public abstract IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp);
    }
    public class WorkstationCapabilitySmeltingWorker : WorkstationCapabilityWorker
    {
        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(WorkstationCapabilityDefOf.Smelting, MaterialRefinementDefOf.Ingots);
        }
    }
    public class WorkstationCapabilityCarpentryWorker : WorkstationCapabilityWorker
    {
        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(WorkstationCapabilityDefOf.Carpentry, MaterialRefinementDefOf.Planks);
        }
    }
    public class WorkstationCapabilityToolMakingWorker : WorkstationCapabilityWorker
    {
        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            return Def.GetDefs<ToolProfileDef>().Select(def => new AddOrderRequest(WorkstationCapabilityDefOf.ToolMaking, def));
        }
    }
    public class WorkstationCapabilityRepairingWorker : WorkstationCapabilityWorker
    {
        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
        {
            yield return new AddOrderRequest(WorkstationCapabilityDefOf.Repairing, null);
        }
    }
    public class WorkstationCapabilityCookingWorker : WorkstationCapabilityWorker
    {
        public override IEnumerable<AddOrderRequest> GetAddOrderRequests(BlockWorkstationComp comp)
            => Def.GetDefs<ConsumableDef>().Select(def => new AddOrderRequest(WorkstationCapabilityDefOf.Cooking, def));
        
    }
}
