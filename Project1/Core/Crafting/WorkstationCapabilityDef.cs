using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Core.AI;
using Project1.Core.Materials;
using Project1.Core.Helpers;
using Project1.Core.Tools;
using Project1.Core.Blocks;

namespace Project1.Core.Crafting
{
    public class WorkstationCapabilityDef(string name, Type workerType) : Def(name)
    {
        public Type ProfileCategory;
        public Def[] SpecificRecipes = [];
        public PlanDef Plan;
        public WorkstationCapabilityWorker Worker = ActivatorSafe<WorkstationCapabilityWorker>.CreateInstance(workerType);

    }
    [EnsureStaticCtorCall]
    internal static class WorkstationCapabilityDefOf
    {
        static public readonly WorkstationCapabilityDef Smelting = new("Smelting", typeof(WorkstationCapabilitySmeltingWorker)) 
        {
            ProfileCategory = typeof(MaterialRefinementDef), 
            SpecificRecipes = [MaterialRefinementDefOf.Ingots],
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef ToolMaking = new("ToolMaking", typeof(WorkstationCapabilityToolMakingWorker))
        {
            ProfileCategory = typeof(ToolProfileDef),
            Plan = PlanDefOf.Crafting
        };
        static public readonly WorkstationCapabilityDef Repairing = new("Repairing", typeof(WorkstationCapabilityRepairingWorker))
        {
            Plan = PlanDefOf.Repairing
        };
        static public readonly WorkstationCapabilityDef Cooking = new("Cooking", typeof(WorkstationCapabilityRepairingWorker))
        {
            Plan = null
        };
        static WorkstationCapabilityDefOf()
        {
            Def.Register(typeof(WorkstationCapabilityDefOf));
        }
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
        {
            throw new NotImplementedException();
        }
    }
    public record AddOrderRequest(WorkstationCapabilityDef WorkstationCapability, Def? ProductDef) { }
}
