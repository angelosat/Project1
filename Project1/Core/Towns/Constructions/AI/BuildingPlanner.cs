using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Labors;
using Project1.Core.Entities;
using Project1.Core.Towns;
using Project1.Core.Towns.Designations;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Materials;
using Project1.Core.AI.Behaviors.Reserve;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Constructions.AI
{
    class BuildingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Builder))
                return null;
            var manager = actor.Map.Town.ConstructionsManager;

            var buildablesReady = manager.GetConstructionsReady().Where(comp => actor.CanReserve(comp.Parent));
            var buildablesUnready = manager.GetConstructionsUnready().Where(comp => actor.CanReserve(comp.Parent));

            var carried = actor.Hauled as Entity;
            if (carried is not null)
            {
                var (target, cell) = GetCarriedUsefulness(actor, buildablesUnready);
                if (target is not null)
                {
                    // can i collect more of what i'm carrying that is near me and is useful for the target or any other target around it?
                    var extraCapacity = carried.StackAvailableSpace;// target.Missing - carried.StackSize;
                    if (extraCapacity > 0)
                    {
                        //(Entity item, int amount) = FindMoreFor(carried, target);
                        (Entity item, int amount) = FindMoreForNew(actor, carried, target);
                        if (item is not null)
                            return new Plan(PlanDefOf.GoHaul, new TargetArgs(item)) { AmountA = amount };
                    }
                    var amountToDeposit = target.Missing;
                    return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, cell)) { AmountA = amountToDeposit, TargetB = new TargetArgs(target.Parent) };
                }
                return null;
            }


            foreach (var comp in buildablesReady)
                foreach (var c in comp.Parent.CellsOccupied)
                    if (actor.CanReach(c))
                        return new Plan(PlanDefOf.Construct, new TargetArgs(actor.Map, c)) { Designation = DesignationDefOf.Construct, TargetB = new TargetArgs(comp.Parent) };

            //var byRefinement = buildables[false].ToLookup(c => c.Requirement.refinement);
            //var byRefinementAndMaterial =
            //        byRefinement.ToDictionary(
            //            grp => grp.Key,
            //            grp => grp
            //                .ToLookup(c => c.Requirement.material)
            //                .ToDictionary(
            //                    g => g.Key,
            //                    g => g.ToList()
            //                )
            //        );
            var byRefinementAndMaterial = new Dictionary<MaterialRefinementDef, Dictionary<MaterialDef, List<BlockConstructionComp>>>();
            foreach (var b in buildablesUnready)
            {
                var r = b.Requirement.refinement;
                var m = b.Requirement.material;
                if (!byRefinementAndMaterial.TryGetValue(r, out var byMaterial))
                    byRefinementAndMaterial[r] = byMaterial = new Dictionary<MaterialDef, List<BlockConstructionComp>>();
                if (!byMaterial.TryGetValue(m, out var list))
                    byMaterial[m] = list = new List<BlockConstructionComp>();
                list.Add(b);
            }
            foreach (var item in actor.Map.Haulables)//.Where(actor.CanReachAndReserve))
            {
                if (item.Def != ItemDefOf.Ingredient)
                    continue;
                var refinementDef = (MaterialRefinementDef)item.Profile;
                if (byRefinementAndMaterial.TryGetValue(refinementDef, out var byMaterial))
                    if (byMaterial.TryGetValue(item.PrimaryMaterial, out var candidates))
                        if (actor.CanReachAndReserve(item))
                            foreach (var comp in candidates.Where(c => actor.CanReserve(c.Parent)))
                                if (comp.Parent.CellsOccupied.Any(c => actor.CanReach(c)))
                                    return new Plan(PlanDefOf.GoHaul, new TargetArgs(item)) { TargetB = new TargetArgs(comp.Parent) };
            }
            return null;
        }
       
        private static int TryCoverDemand(BlockConstructionComp target, Entity carried, IEnumerable<BlockConstructionComp> unready)
        {
            int totalDemand = target.Missing, maxDemand = carried.StackMax;
            foreach(var comp in unready)
            {
                totalDemand += comp.DemandFor(carried);
                if (totalDemand > maxDemand)
                    return maxDemand;
            }
            return totalDemand;
        }
        private static (Entity item, int amount) FindMoreForNew(Actor actor, Entity carried, BlockConstructionComp target)
        {
            if (carried.IsStackFull)
                return default;

            var allUnready = target.Map.Town.ConstructionsManager.GetConstructionsUnready();
            var totalDemand = TryCoverDemand(
                target, 
                carried, 
                allUnready
                    .SkipWhile(c => c == target)
                    .Where(c => target.Global.GetRadial(2)
                    .Contains(c.Global)));

            var remaining = totalDemand - carried.StackSize;
            if (remaining == 0)
                return default;

            var mapItems = target.Map.Entities;
            foreach (var item in mapItems)
            {
                var itemDistance = Vector3.DistanceSquared(actor.Global, item.Global);
                var constructionDistance = Vector3.DistanceSquared(actor.Global, target.Global);

                if (itemDistance > constructionDistance) continue;
                if (!carried.CanAbsorb(item)) continue;
                var amountToTake = Math.Min(item.StackSize, Math.Min(remaining, carried.StackAvailableSpace));
                return (item, amountToTake);
            }
            return default;
        }
        private static (BlockConstructionComp target, IntVec3 cell) GetCarriedUsefulness(Actor actor, IEnumerable<BlockConstructionComp> buildings)
        {
            var item = actor.Hauled as Entity;
            foreach (var b in buildings)
            {
                if (!b.Accepts(item)) continue;
                foreach (var cell in b.Parent.CellsOccupied)
                    if (actor.CanReach(cell))
                        return (b, cell);
            }
            return (null, default);
        }
    }
}
