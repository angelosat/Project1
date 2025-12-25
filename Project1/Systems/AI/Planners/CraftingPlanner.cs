using Start_a_Town_;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Start_a_Town_
{
    class CraftingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Craftsman))
                return null;
            var map = actor.Map;

            // Guard: don’t interfere with other planners
            if (actor.Hauled != null && !IsCarriedItemRelevantForAnyOrder(actor))
                return null;

            var manager = map.Town.CraftingManagerNew;

            var allOrders = manager.GetAllOrdersUnsorted();
            foreach (var order in allOrders)
            {
                //var (result, allocations) = TryCollectIngredients(actor, order);
                var result = TryCollectIngredients(actor, order);
                // If the crafting flow cannot be completed fully, abort planner
                //if (!result)
                if (result.State == CraftingOrderState.NotEnoughItems)
                    return null;
                var carried = actor.Hauled as Entity;

                //if(!allocations.Any())
                if (result.State == CraftingOrderState.ReadyToCraft && carried == null)
                {
                    var plan = new Plan(PlanDefOf.Crafting, new TargetArgs(actor.Map, order.Workstation.Parent.OriginGlobal)) { Order = order };
                    foreach(var inSlot in result.InSlots)
                        plan.AddTarget(TargetIndex.A, inSlot.entity);
                    return plan;
                }
                var allocations = result.ToTransfer;
                if (carried != null)
                {
                    if (CanDeliverCarriedItemToOrder(actor, order, out var carriedTargetSlot))
                    {
                        //return new Plan(TaskDefOf.GoPlace, new TargetArgs(carried), new TargetArgs(actor.Map, carriedTargetSlot));
                        return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, carriedTargetSlot));
                    }
                    else if (IsCarriedItemUsefulForOrder(actor, order))
                    {
                        // Use precomputed allocation from TryCollectIngredients
                        var allocation = allocations
                            .SelectMany(a => a.pair)
                            .FirstOrDefault(a => a.stack == carried);

                        if (allocation.stack != null)
                            return new Plan(PlanDefOf.GoHaul, new TargetArgs(allocation.stack)) { AmountA = allocation.quantity };

                        // should not reach here if allocations are accurate
                        throw new InvalidOperationException();
                    }
                    else
                    {
                        return null; // irrelevant carried item → fallback planner will drop it
                    }
                }
                // If carried is null, just go find a world item to pick up:
                var nextItem = FindNextWorldItemForOrder(actor, order, allocations);
                if(nextItem != null)
                    return new Plan(PlanDefOf.GoHaul, new TargetArgs(nextItem.Value.stack)) { AmountA = nextItem.Value.quantity };


                //var result = TryCollectIngredients(actor, order);
                //if (!result.result)
                //    continue;

                //var cachedReqs = order.GetIngredientRequirements();
                //var carried = actor.Hauled;

                //foreach (var ing in result.allocations)
                //    foreach (var (entity, quantity) in ing.pair)
                //        return new Plan(TaskDefOf.PickUp) { TargetA = entity, AmountA = quantity };
                //--------
                //var task = new AITask(TaskDefOf.Crafting);
                //task.Order = order;
                //foreach (var ing in result.allocations)
                //    task.AddTargets(TaskBehaviorCrafting.IngredientIndex, ing.Select(i => (new TargetArgs(i.stack), i.quantity)));
                //task.SetTarget(TaskBehaviorCrafting.WorkstationIndex, new TargetArgs(actor.Map, order.Workstation.Global));
                //return task;

                // Fallthrough: all ingredients are already on workstation
                //return new Plan(TaskDefOf.Crafting, new TargetArgs(actor.Map, order.Workstation.Global));
            }
            return null;
        }
        static (Entity stack, int quantity)? FindNextWorldItemForOrder(Actor actor, OrderSettings order, IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> collectResult)
        {
            foreach (var (pair, slot) in collectResult)
            {
                foreach (var (stack, qty) in pair)
                {
                    if (stack != actor.Hauled && qty > 0)
                        return (stack, qty);
                }
            }
            return null;
        }
        private static bool CanDeliverCarriedItemToOrder(Actor actor, OrderSettings order, out IntVec3 targetSlot)
        {
            targetSlot = default;

            var carried = actor.Hauled as Entity;
            if (carried == null)
                return false;

            foreach (var req in order.GetIngredientRequirements())
            {
                //if (req.MatchesPartial(carried, out var _))
                //{
                //    targetSlot = req.Slot;
                //    return true;
                //}
                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);
                int slotQuantity = slotEntities.Sum(e => req.MatchesPartial(e, out var q) ? q : 0);

                if (req.Matches(carried) && carried.StackSize + slotQuantity >= req.Quantity)
                {
                    targetSlot = req.Slot;
                    return true;
                }
                //return false;
            }

            return false;
        }
        bool IsCarriedItemUsefulForOrder(Actor actor, OrderSettings order)
        {
            var carried = actor.Hauled as Entity;
            if (carried == null)
                return false;

            foreach (var req in order.GetIngredientRequirements())
            {
                if (!req.Matches(carried))
                    continue;

                int missing = req.Quantity;

                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);
                foreach (var e in slotEntities)
                    if (req.MatchesPartial(e, out var used))
                        missing -= used;

                if (missing > 0)
                    return true;
            }
            return false;
        }
       

        bool IsCarriedItemRelevantForAnyOrder(Actor actor)
        {
            var carried = actor.Hauled as Entity;
            if (carried == null)
                return false;

            foreach (var order in actor.Map.Town.CraftingManagerNew.GetAllOrdersUnsorted())
                foreach (var req in order.GetIngredientRequirements())
                    if (req.Matches(carried))
                        return true;

            return false;
        }
        protected Plan TryAssignTaskOld(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Craftsman))
                return null;
            var map = actor.Map;

            var manager = actor.Map.Town.CraftingManager;

            var allOrders = manager.ByWorkstationNew();

            var itemAmounts = new List<Dictionary<TargetArgs, int>>();
            var materialsUsed = new Dictionary<string, Entity>();
            foreach (var bench in allOrders)
            {
                var benchglobal = bench.Key;
                if (map.Town.ShopManager.GetShop(benchglobal) != null)
                    continue;
         
                var opPos = map.GetFrontOfBlock(benchglobal);
                if (!actor.CanReserve(opPos)
                    || !map.IsStandableIn(opPos)
                    || !actor.CanReach(opPos))
                    continue;


                foreach (var order in bench.Value)
                {
                    if (!actor.HasJob(order.Reaction.Labor))
                        continue;
                    if (!actor.CanReserve(benchglobal))
                        continue;
                    if (!actor.CanReserve(benchglobal.Above))
                        continue;
                    var operatingPos = map.GetCell(benchglobal).GetInteractionSpots(map, benchglobal).First();
                    if (!actor.CanStandInNew(operatingPos))
                        continue;
                    if (actor.Def.OccupyingCellsStandingWithBase(operatingPos).Any(c => !actor.CanReserve(c)))
                        continue;
                    if (!(order.IsActive && order.IsCompletable()))
                        continue;
                    if(order.UnfinishedItem is Entity unf && unf.IsSpawned && unf.GetComponent<UnfinishedItemComp>().Creator == actor && actor.CanReserve(unf))
                    {
                        if (!TaskHelper.TryClearArea(actor, benchglobal.Above, itemAmounts.SelectMany(d => d.Keys.Select(t => t.Object)).Distinct(), out var clearTask))
                        {
                            if (clearTask is null)
                                continue;
                            return clearTask;
                        }
                        var workstationTarget = new TargetArgs(map, benchglobal);
                        var task = new Plan(PlanDefOf.Crafting);
                        task.OrderOld = order;
                        task.AddTarget(TaskBehaviorCrafting.IngredientIndex, unf, 1);
                        task.SetTarget(TaskBehaviorCrafting.WorkstationIndex, workstationTarget);
                        if (order.Reaction.Labor is not null)
                            task.Tool = FindTool(actor, order.Reaction.Labor);
                        return task;
                    }
                    if (TryFindAllIngredients(actor, ref itemAmounts, materialsUsed, order))
                    {
                        /// clear workstation first and enqueue the crafting task?
                        if (!TaskHelper.TryClearArea(actor, benchglobal.Above, itemAmounts.SelectMany(d => d.Keys.Select(t => t.Object)).Distinct(), out var clearTask))
                        {
                            if (clearTask is null)
                                continue;
                            return clearTask;
                        }

                        var workstationTarget = new TargetArgs(map, benchglobal);
                        var task = new Plan(PlanDefOf.Crafting);
                        foreach (var dic in itemAmounts)
                            foreach (var itemAmount in dic)
                                task.AddTarget(TaskBehaviorCrafting.IngredientIndex, itemAmount.Key, itemAmount.Value);
                        task.SetTarget(TaskBehaviorCrafting.WorkstationIndex, workstationTarget);
                        task.OrderOld = order;
                        if (order.Reaction.Labor is not null)
                            task.Tool = FindTool(actor, order.Reaction.Labor);

                        return task;
                    }
                }
            }
            return null;
        }
        private static bool AllReagentsAvailable(GameObject actor, List<GameObject> allObjects, ref List<Dictionary<TargetArgs, int>> itemAmounts, Dictionary<string, int> materialsUsed, CraftOrder order)
        {
            return AllReagentsAvailable(actor, allObjects, ref itemAmounts, materialsUsed, order);
        }
        enum CraftingOrderState
        {
            NotEnoughItems,      // No ingredients available at all
            NeedsTransfer,       // Ingredients exist on the map but not in slots
            ReadyToCraft         // All required ingredients are already in slots
        }
        struct CraftingCollectionResult
        {
            public CraftingOrderState State;  // NotEnoughItems, NeedsTransfer, ReadyToCraft
            public IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> ToTransfer; // map items to move to slots
            public IEnumerable<(IntVec3 slot, Entity entity)> InSlots;       // items already in slots

            public CraftingCollectionResult(CraftingOrderState state, IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> toTransfer, IEnumerable<(IntVec3 slot, Entity entity)> inSlots)
            {
                State = state;
                ToTransfer = toTransfer;
                InSlots = inSlots;
            }
        }
        private static CraftingCollectionResult TryCollectIngredients(Actor actor, OrderSettings order)
        {
            var mapEntities = actor.Map.GetEntities<Entity>();
            Dictionary<Entity, int> allocatedSoFar = [];
            List<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)> allFound = [];
            List<(IntVec3 slot, Entity entity)> inSlots = [];
            foreach (var req in order.GetIngredientRequirements())
            {
                var missingQuantity = req.Quantity;
                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);

                //if (slotEntities.Any(entity => req.Matches(entity) && req.Quantity == entity.StackSize))
                if (slotEntities.FirstOrDefault(entity => req.Matches(entity) && req.Quantity == entity.StackSize) is Entity inSlot)
                {
                    inSlots.Add((req.Slot, inSlot));
                    break;
                }
                var carried = actor.Hauled as Entity;
                if (missingQuantity > 0 && carried != null && req.Matches(carried))
                {
                    var used = Math.Min(carried.StackSize, missingQuantity);
                    missingQuantity -= used;
                }

                Debug.Assert(missingQuantity >= 0);
                if (missingQuantity == 0)
                    continue;
                var validStacks = mapEntities.Where(req.Matches);
                var allocation = AllocateRequirement(actor, validStacks, missingQuantity, allocatedSoFar);
                if (allocation is null)
                    //return (false, Enumerable.Empty<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)>());
                    return new CraftingCollectionResult(CraftingOrderState.NotEnoughItems, null, null);
                allFound.Add((allocation, req.Slot));
            }
            if (!allFound.Any())
                return new(CraftingOrderState.ReadyToCraft, null, inSlots);
            return new(CraftingOrderState.NeedsTransfer, allFound, null);
            //return (true, allFound);
        }
        //private static (bool result, IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> allocations) TryCollectIngredients(Actor actor, OrderSettings order)
        //{
        //    var mapEntities = actor.Map.GetEntities<Entity>();
        //    Dictionary<Entity, int> allocatedSoFar = [];
        //    List<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)> allFound = [];
        //    foreach (var req in order.GetIngredientRequirements())
        //    {
        //        var missingQuantity = req.Quantity;
        //        var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);

        //        if (slotEntities.Any(entity => req.Matches(entity) && req.Quantity == entity.StackSize))
        //            break;

        //        var carried = actor.Hauled as Entity;
        //        if (missingQuantity > 0 && carried != null && req.Matches(carried))
        //        {
        //            var used = Math.Min(carried.StackSize, missingQuantity);
        //            missingQuantity -= used;
        //        }

        //        Debug.Assert(missingQuantity >= 0);
        //        if (missingQuantity == 0)
        //            continue;
        //        var validStacks = mapEntities.Where(req.Matches);
        //        var allocation = AllocateRequirement(actor, validStacks, missingQuantity, allocatedSoFar);
        //        if (allocation is null)
        //            return (false, Enumerable.Empty<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)>());
        //        allFound.Add((allocation, req.Slot));
        //    }
        //    return (true, allFound);
        //}
        private static (bool result, IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> allocations) TryCollectIngredientsLessOld(Actor actor, OrderSettings order)
        {
            var mapEntities = actor.Map.GetEntities<Entity>();
            Dictionary<Entity, int> allocatedSoFar = [];
            List<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)> allFound = [];
            foreach (var req in order.GetIngredientRequirements())
            {
                Entity primaryMatch = null;
                var missingQuantity = req.Quantity;
                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);
                foreach(var entity in slotEntities)
                {
                    if(req.MatchesPartial(entity, out var qtyUsed))
                    {
                        primaryMatch = entity;
                        missingQuantity -= qtyUsed;
                        break;
                    }
                }
                Debug.Assert(missingQuantity >= 0);
                if (missingQuantity == 0)
                    continue;
                var validStacks = mapEntities.Where(req.Matches);
                var allocation = AllocateRequirement(actor, validStacks, missingQuantity, allocatedSoFar);
                if (allocation is null)
                    return (false, Enumerable.Empty<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)>());
                allFound.Add((allocation, req.Slot));
            }
            return (true, allFound);
        }
        private static (bool result, IEnumerable<IEnumerable<(Entity stack, int quantity)>> allocations) TryCollectIngredientsOld(Actor actor, OrderSettings order)
        {
            var mapEntities = actor.Map.GetEntities<Entity>();
            Dictionary<Entity, int> allocatedSoFar = [];
            List<IEnumerable<(Entity stack, int quantity)>> allFound = [];
            foreach (var req in order.GetIngredientRequirements())
            {
                var missingQuantity = req.Quantity;
                var validStacks = mapEntities.Where(e => req.Matches(e));
                var allocation = AllocateRequirement(actor, validStacks, missingQuantity, allocatedSoFar);
                if (allocatedSoFar is not null)
                allFound.Add(allocation);
            }
            return (true, allFound);
        }
        /// <summary>
        /// Attempts to allocate a specific ingredient requirement from available stacks, considering reservations.
        /// Returns null if the required quantity cannot be fully satisfied.
        /// </summary>
        /// <param name="actor">The actor performing the crafting.</param>
        /// <param name="validStacks">Candidate item stacks that match the ingredient requirement.</param>
        /// <param name="requiredQuantity">Total quantity needed for this ingredient.</param>
        /// <param name="allocatedSoFar">Tracks tentative allocations for this run to prevent double-counting.</param>
        /// <returns>List of (stack, quantity) tuples if allocation succeeds; null if insufficient resources.</returns>
        private static List<(Entity stack, int quantity)> AllocateRequirement(
            Actor actor,
            IEnumerable<Entity> validStacks,
            int requiredQuantity,
            Dictionary<Entity, int> allocatedSoFar)
        {
            // Access the reservation manager to respect existing reservations
            var reservationManager = actor.Map.Town.ReservationManager;

            // This list will hold the final allocation for this ingredient
            var allocation = new List<(Entity, int)>();

            foreach (var stack in validStacks)
            {
                // How much of this stack has already been tentatively allocated in this allocation run
                var allocated = allocatedSoFar.GetValueOrDefault(stack, 0);

                // How much of this stack is available according to the global reservation system
                var unreservedQuantity = reservationManager.GetUnreservedAmount(new TargetArgs(stack));

                // Compute the true available amount for this allocation attempt
                var availableForAllocation = unreservedQuantity - allocated;

                // Assert that logic is consistent; negative availability indicates a bug
                Debug.Assert(availableForAllocation >= 0);

                // Skip stacks that have no available units
                if (availableForAllocation == 0)
                    continue;

                // Determine how much we can take from this stack without exceeding required quantity
                var take = Math.Min(requiredQuantity, availableForAllocation);

                // Assert that take is positive; zero or negative indicates logic error
                Debug.Assert(take > 0);
                if (take == 0)
                    continue;

                // Add this allocation to the list
                allocation.Add((stack, take));

                // Update the tentative allocation to prevent double-counting in this run
                allocatedSoFar[stack] = allocated + take;

                // Reduce the remaining quantity we still need
                requiredQuantity -= take;

                // Sanity check: remaining quantity should never go negative
                Debug.Assert(requiredQuantity >= 0);

                // Stop once we have allocated the full required quantity
                if (requiredQuantity == 0)
                    break;
            }

            // If we still need more than we could allocate, return null to indicate failure
            return requiredQuantity > 0 ? null : allocation;
        }
        private static bool TryFindAllIngredients(Actor actor, ref List<Dictionary<TargetArgs, int>> itemAmounts, Dictionary<string, Entity> materialsUsed, CraftOrder order)
        {
            var map = actor.Map;
            
            var allObjects = order.Input is Stockpile input ? input.Contents.OfType<Entity>() : map.GetEntities().OfType<Entity>().ToArray();

            //var handled = new HashSet<GameObject>();
            Dictionary<Entity, int> alreadyFound = new();
            List<Dictionary<Entity, int>> trips = new();

            foreach (var reagent in order.Reaction.Reagents)
            {
                var handled = new HashSet<GameObject>();

                var validStacks = (from stack in allObjects
                                   where stack.IsHaulable
                                   where !handled.Contains(stack)
                                   where order.IsItemAllowed(reagent.Name, stack)
                                   select stack).SortByReachableRegionDistance(actor); // closest to actor or workstation?
                var reqAmount = reagent.Quantity; 
                var totalfound = 0;
                var currentStack = 0;
                Entity materialID = null;
                Dictionary<TargetArgs, int> targetsAmounts = new();
                var currentTrip = new Dictionary<GameObject, int>();
                foreach (var stack in validStacks)
                {
                    handled.Add(stack);
                    var unreservedAmount = actor.GetUnreservedAmount(stack);
                    if (alreadyFound.ContainsKey(stack))
                        unreservedAmount -= alreadyFound[stack];
                    if (unreservedAmount == 0)
                        continue;
                    reqAmount = reagent.Quantity * stack.Def.StackDimension;
                    var amountToPick = Math.Min(unreservedAmount, reqAmount - totalfound);
                    totalfound += amountToPick;
                    targetsAmounts.Add(new TargetArgs(stack), amountToPick);

                    if (alreadyFound.ContainsKey(stack))
                        alreadyFound[stack] += amountToPick;
                    else
                        alreadyFound[stack] = amountToPick;
                    var newAmount = alreadyFound[stack] + amountToPick;
                    if (newAmount == stack.StackMax)
                    {
                        trips.Add(alreadyFound);
                        alreadyFound = new Dictionary<Entity, int>();
                    }
                    if (currentStack > stack.StackMax)
                        throw new Exception();
                    materialID = stack;
                    if (totalfound == reqAmount)
                        break;

                    if (totalfound > reqAmount)
                        throw new Exception();
                    currentStack += amountToPick;
                    if (currentStack == stack.StackMax)
                    {
                        itemAmounts.Add(targetsAmounts);
                        targetsAmounts = new Dictionary<TargetArgs, int>();
                    }
                }
                if (totalfound < reqAmount)
                    return false;
                materialsUsed.Add(reagent.Name, materialID);
                itemAmounts.Add(targetsAmounts);
            }
            if (alreadyFound.Any())
                trips.Add(alreadyFound);
            itemAmounts.Clear();
            foreach (var i in trips)
                itemAmounts.Add(i.ToDictionary(o => new TargetArgs(o.Key), o => o.Value));
            return true;
        }
    }
}
