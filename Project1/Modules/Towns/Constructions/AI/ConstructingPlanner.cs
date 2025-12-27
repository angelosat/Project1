using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ConstructingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Builder))
                return null;
            var manager = actor.Map.Town.ConstructionsManager;

            var buildablesReady = manager.GetConstructionsReady();
            var buildablesUnready = manager.GetConstructionsUnready();

            var carried = actor.Hauled as Entity;
            if (carried is not null)
            {
                var (target, cell) = GetCarriedUsefulness(actor, buildablesUnready);
                if (target is not null)
                {
                    var extraCapacity = target.Missing - carried.StackSize;
                    if (extraCapacity > 0)
                    {
                        (Entity item, int amount) = FindMoreFor(carried, target);
                        if (item is not null)
                            return new Plan(PlanDefOf.GoHaul, new TargetArgs(item)) { AmountA = amount };
                    }
                    var amountToDeposit = target.Missing;
                    return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, cell)) { AmountA = amountToDeposit };
                }
                return null;
            }


            foreach (var comp in buildablesReady)
                foreach (var c in comp.Parent.CellsOccupied)
                    if (actor.CanReach(c))
                        return new Plan(PlanDefOf.Construct, new TargetArgs(actor.Map, c));

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
            foreach (var item in actor.Map.Entities)
            {
                if (item.Def != ItemDefOf.Ingredient)
                    continue;
                var refinementDef = (MaterialRefinementDef)item.Profile;
                if (byRefinementAndMaterial.TryGetValue(refinementDef, out var byMaterial))
                    if (byMaterial.TryGetValue(item.PrimaryMaterial, out var candidates))
                        if (actor.CanReach(item) && actor.CanReserve(item))
                            foreach (var comp in candidates)
                                if (comp.Parent.CellsOccupied.Any(c => actor.CanReach(c)))
                                    return new Plan(PlanDefOf.GoHaul, new TargetArgs(item));
            }
            return null;
        }

        private static (Entity item, int amount) FindMoreFor(Entity carried, BlockConstructionComp target)
        {
            var mapItems = target.Map.Entities;
            foreach(var item in mapItems)
            {
                if (!target.Accepts(item as Entity)) continue;
                if (!carried.CanAbsorb(item)) continue;
                var amountToTake = Math.Min(item.StackSize, Math.Min(target.Missing, carried.StackAvailableSpace));
                return (item as Entity, amountToTake);
            }
            return default;
        }

        private static (BlockConstructionComp comp, IntVec3 target) IsItemUsefulForAnyConstruction(Actor actor, IEnumerable<BlockConstructionComp> buildings)
        {
            var item = actor.Hauled as Entity;
            //return buildings.First(b => b.Accepts(carried) && );
            foreach(var b in buildings.Where(b=>b.Accepts(item)))
            {
                foreach (var cell in b.Parent.CellsOccupied)
                    if (actor.CanReach(cell))
                        return (b, cell);
            }
            return (null, default);
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
        Entity ScanForIngredients(BlockConstructionComp comp)
        {
            return null;
        }
        protected Plan TryPlanOld(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Builder))
                return null;
            var manager = actor.Map.Town.ConstructionsManager;
            // TODO: if was previously building, continue building other available nearby unfinished constructions, instead of stopping and delivering materials

            var all = manager.GetAllBuildableCurrently();
            var allOrdered = all
                .OrderByReachableRegionDistance(actor);
            var lastBehav = actor.GetLastBehavior();
            var preferBuild = lastBehav != null && lastBehav is TaskBehaviorConstruct;

            foreach (var closest in allOrdered)
            {
                if (!actor.CanReserve(closest)) // filter them here or before ordering them by distance?
                    continue;

                if (actor.Map.GetBlockEntity(closest) is not IConstructible blockEntity)
                    continue; // because the list contains other block types as well

                if(!TryClearArea(actor, closest, out var clearTask))
                {
                    if (clearTask is null)
                        continue;
                    return clearTask;
                }

                if (blockEntity.IsReadyToBuild(out ItemDef def, out MaterialDef mat, out int amount))
                {
                    var buildtask = TryBuild(actor, closest, blockEntity);
                    if (buildtask != null)
                        return buildtask;
                    else
                        continue;
                }
                if (preferBuild)
                {
                    continue;
                }

                var deliverTask = TryDeliverMaterialNewNew(actor, closest, all, def, mat);
                if (deliverTask != null)
                    return deliverTask;
                else
                    continue;
            }
           
            return null;
        }
        

        static public bool IsOperatable(Actor actor, Vector3 global)
        {
            var map = actor.Map;
            var nodes = map.Regions.GetPotentialNodesAroundDestination(actor.Physics.Reach, global);
            var any = nodes.Any(n => map.IsStandableOn(n.Global));
            return any;
        }
        Plan TryBuild(Actor actor, Vector3 global, IConstructible cachedBlockEntity )
        {
            if (!actor.CanReserve(global))
                return null;
            if (!IsOperatable(actor, global))
                return null;
           
            /////move aside any obstructing items
            //var items = actor.Map.GetObjects(global);
            //foreach(var i in items)
            //{
            //    if (i is not Entity ientity)
            //        continue;
            //    if(i is Plant && actor.CanReserve(i))
            //    {
            //        var plantCutTask = new AITask(TaskDefOf.Chopping, i)
            //        {
            //            Tool = FindTool(actor, JobDefOf.Lumberjack)
            //        };
            //        return plantCutTask;
            //    }
            //    var haulAsideTask = TaskHelper.TryHaulAside(actor, ientity);
            //    if (haulAsideTask != null)
            //        return haulAsideTask;
            //}

            //if (items.Any()) // return null if failure to return haul aside task
            //    return null;

            var buildtask = new Plan(PlanDefOf.Construct);
            buildtask.SetTarget(TaskBehaviorConstruct.ConstructionsID, new TargetArgs(actor.Map, global));

            var construction = cachedBlockEntity as BlockConstructionEntity;
            if (construction.Product.Block.BuildProperties.ToolSensitivity > 0)
                FindTool(actor, buildtask, JobDefOf.Builder);

            return buildtask;
        }
        /// <summary>
        /// returns true if area is clear. if it's not clear, returns false and tries to find an appropriate task to clear it. if the out task is null, it means that it's unclearable
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="global"></param>
        /// <param name="clearAreaTask"></param>
        /// <returns></returns>
        static bool TryClearArea(Actor actor, IntVec3 global, out Plan clearAreaTask)
        {
            clearAreaTask = null;
            //var items = actor.Map.GetObjects(global);
            var items = actor.Map.GetObjectsOccupyingCell(global);
            if (!items.Any())
                return true;
            foreach (var i in items)
            {
                if (i is not Entity ientity)
                    continue;
                if (i is Plant && actor.CanReserve(i))
                {
                    var plantCutTask = new Plan(PlanDefOf.Chop, i)
                    {
                        Tool = FindTool(actor, JobDefOf.Lumberjack)
                    };
                    clearAreaTask = plantCutTask;
                    return false;
                }
                var haulAsideTask = TaskHelper.TryHaulAside(actor, ientity);
                if (haulAsideTask != null)
                {
                    clearAreaTask= haulAsideTask;
                    return false;
                }
            }
            return false;
        }
        Plan TryDeliverMaterialNewNew(Actor actor, Vector3 origin, IEnumerable<IntVec3> allConstructions, ItemDef ingredientDef, MaterialDef ingredientMat)
        {
            if (!IsOperatable(actor, origin))
                return null;
            var task = new Plan(PlanDefOf.DeliverMaterials);
            var allObjects = actor.Map.GetEntities();
            var enduranceLimit = Math.Min(actor.GetHaulStackLimitFromEndurance(ingredientDef), ingredientDef.StackCapacity);
            var maxDeliverable = 0;
            var similarNearbyConstructions = GetNearbyConstructionsWithSameMaterialNewNewInclusive(actor, allConstructions, origin, ingredientDef);
            var constrEnum = similarNearbyConstructions.GetEnumerator();
            while (maxDeliverable < enduranceLimit && constrEnum.MoveNext())
            {
                var n = constrEnum.Current;
                var constr = actor.Map.GetBlockEntity(n) as IConstructible;
                maxDeliverable += constr.GetMissingAmount(ingredientDef);
            }

            var remaining = Math.Min(enduranceLimit, maxDeliverable);
            var found = 0;

            var objenum = allObjects.GetEnumerator();
            while (objenum.MoveNext() && remaining > 0)
            {
                var o = objenum.Current;
                if (ingredientDef != o.Def)
                    continue;
                if (o.PrimaryMaterial != ingredientMat)
                    continue;
                var unreservedAmount = actor.GetUnreservedAmount(o);
                if (unreservedAmount == 0)
                    continue;
                if (!actor.CanReach(o.Global))
                    continue;
                var amountToPick = Math.Min(remaining, unreservedAmount);
                found += amountToPick;
                remaining -= amountToPick;
                if (remaining < 0)
                    throw new Exception();
                task.AddTarget(TaskBehaviorDeliverMaterials.MaterialID, new TargetArgs(o), amountToPick);
            }
            if (found == 0)
                return null;
            remaining = found;
            constrEnum = similarNearbyConstructions.GetEnumerator();
            while (constrEnum.MoveNext() && remaining > 0)
            {
                var currentDelivery = constrEnum.Current;
                var missingAmount = (actor.Map.GetBlockEntity(currentDelivery) as IConstructible).GetMissingAmount(ingredientDef);
                var toDrop = Math.Min(remaining, missingAmount);

                if (toDrop == 0)
                    continue;
                task.AddTarget(TaskBehaviorDeliverMaterials.DestinationID, new TargetArgs(actor.Map, currentDelivery), toDrop);
                remaining -= toDrop;
            }
            
            return task;
        }
        
        static IEnumerable<Vector3> GetNearbyConstructionsWithSameMaterialNewNewInclusive(Actor actor, IEnumerable<IntVec3> allBuildable, Vector3 origin, ItemDef def)
        {
            yield return origin;
            var map = actor.Map;
            var currentBlockEntity = map.GetBlockEntity(origin);
            var maxRangeSquared = 25;
            var distinctDesignations = new HashSet<BlockEntity>
            {
                currentBlockEntity
            };

            foreach (var designation in allBuildable)
            {
                if (!actor.CanReserve(designation))
                    continue;
                var entity = map.GetBlockEntity(designation);
                if (entity == null)
                    throw new Exception();
                if (distinctDesignations.Contains(entity))
                    continue;
                if (!(entity as IConstructible)?.IsValidHaulDestination(def) ?? false)
                    continue;
                if (!actor.CanReach(designation))
                    continue;
                if (Vector3.DistanceSquared(designation, origin) > maxRangeSquared)
                    continue;
                if (!IsOperatable(actor, designation))
                    continue;
                yield return designation;
            }
        }
    }
}
