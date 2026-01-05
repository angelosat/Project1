using Start_a_Town_;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Start_a_Town_.OrderSettings;
using static Start_a_Town_.OrderSettings.OrderFeasibilityResult;

namespace Start_a_Town_
{
    class CraftingPlanner : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Craftsman))
                return null;

            var map = actor.Map;
            var carried = actor.Hauled as Entity;
            var manager = map.Town.CraftingManagerNew;

            // Gather all pending, reachable orders
            var allOrders = manager.GetAllOrdersUnsorted()
                .Where(o => o.Pending && actor.CanReachAndReserve(o.Workstation.Parent));

            // Guard: don't interfere if carrying irrelevant item
            if (carried != null && !allOrders.Any(o => o.Matches(carried)))
                return null;

            

            foreach (var order in allOrders)
            {
                // Track slots we want to exclude (already being deposited into by others)
                var workstationSlots = order.Workstation.Parent.CellsOccupied;
                var excludedSlots = workstationSlots.Select(c => c.Above).Where(c => !actor.CanReachAndReserve(c)).ToHashSet();
                if (excludedSlots.Count == workstationSlots.Count)
                    continue;

                var mapEntities = map.GetEntities<Entity>().Where(i=>i.Def == ItemDefOf.Ingredient && actor.CanReachAndReserve(i));
                if (carried is not null)
                    mapEntities = mapEntities.Prepend(carried);
                var candidates = mapEntities.ToList();

                // Build candidate pool (carried first if exists)
                //var candidates = carried != null
                //    ? new List<Entity> { carried }.Concat(map.GetEntities<Entity>().Where(actor.CanReachAndReserve)).ToList()
                //    : map.GetEntities<Entity>().Where(actor.CanReachAndReserve).ToList();


                // Evaluate feasibility with exclusions
                var feasibility = order.IsFeasibleNew(candidates, excludedSlots, carried);

                if (feasibility.State == CraftingOrderState.NotEnoughItems)
                    continue;

                // All slots already satisfied
                if (feasibility.State == CraftingOrderState.ReadyToCraft && 
                    carried == null && 
                    actor.CanReachAndReserve(order.Workstation.Parent))
                    //feasibility.ArmedSlots.All(i => actor.CanReachAndReserve(i.Entity)))
                {
                    var plan = new Plan(PlanDefOf.Crafting, new TargetArgs(map, order.Workstation.Parent.OriginGlobal)) { Order = order, TargetB = new TargetArgs(order.Workstation.Parent) };

                    foreach (var allocation in feasibility.ArmedSlots)
                        plan.AddTarget(TargetIndex.A, allocation.Entity);
                    return plan;
                }

                // Carried item can be deposited
                if (carried != null)
                {
                    var carriedAlloc = feasibility.Allocations
                        .FirstOrDefault(a => a.Entity == carried);

                    if (carriedAlloc.Entity != null)
                    {
                        if (carried.StackSize >= carriedAlloc.Quantity)
                        {
                            // Fully satisfies → deposit now
                            return new Plan(PlanDefOf.GoPlace,
                                new TargetArgs(map, carriedAlloc.Slot))
                            {
                                AmountA = carriedAlloc.Quantity
                            };
                        }
                        else
                        {
                            // Partially satisfies → gather remainder first
                            var remainderAlloc = feasibility.Allocations
                                .First(a => a.Slot == carriedAlloc.Slot && a.Entity != carried);

                            return new Plan(PlanDefOf.GoHaul,
                                new TargetArgs(remainderAlloc.Entity))
                            {
                                AmountA = remainderAlloc.Quantity
                            };
                        }
                    }
                }

                // Otherwise, pick up next needed item
                if (carried != null)
                {
                    var correctAlloc = feasibility.Allocations.FirstOrDefault(a => carried.CanAbsorb(a.Entity));
                    if (correctAlloc.Entity != null)
                    {
                        return new Plan(PlanDefOf.GoHaul, new TargetArgs(correctAlloc.Entity))
                        {
                            AmountA = correctAlloc.Quantity
                        };
                    }
                }

                foreach (var alloc in feasibility.Allocations)
                {
                    return new Plan(PlanDefOf.GoHaul, new TargetArgs(alloc.Entity))
                    {
                        AmountA = alloc.Quantity
                    };
                }
                // Otherwise, pick up next needed item
                //foreach (var alloc in feasibility.Allocations)
                //{
                //    // Skip entities already carried
                //    if (alloc.Entity == carried)
                //        continue;

                //    return new Plan(PlanDefOf.GoHaul, new TargetArgs(alloc.Entity))
                //    {
                //        AmountA = alloc.Quantity
                //    };
                //}
            }

            return null;
        }


        protected Plan TryPlanPrevious(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Craftsman))
                return null;
            var map = actor.Map;
            var carried = actor.Hauled as Entity;

            var manager = map.Town.CraftingManagerNew;
            var allOrders = manager.GetAllOrdersUnsorted()
               .Where(o => o.Pending &&
               actor.CanReachAndReserve(o.Workstation.Parent));

            // Guard: don’t interfere with other planners
            if (carried is not null && !allOrders.Any(o => o.Matches(carried)))
                return null;
           
            foreach (var order in allOrders)
            {
                var result = TryCollectIngredientsNew(actor, order);

                // If the crafting flow cannot be completed fully, abort planner
                if (result.State == CraftingOrderStateOld.NotEnoughItems)
                    continue;

                if (result.State == CraftingOrderStateOld.ReadyToCraft && carried is null)
                    {
                    var plan = new Plan(PlanDefOf.Crafting, new TargetArgs(actor.Map, order.Workstation.Parent.OriginGlobal)) { Order = order };
                    foreach(var (slot, entity) in result.InSlots)
                        plan.AddTarget(TargetIndex.A, entity);
                    return plan;
                }

                var allocations = result.ToTransfer;
                if (carried is not null)
                {
                    if (CanDeliverCarriedItemToOrder(actor, order, out var carriedTargetSlot))
                    {
                        return new Plan(PlanDefOf.GoPlace, new TargetArgs(actor.Map, carriedTargetSlot));
                    }
                    else if (order.MatchesPartial(carried, out _))// (IsCarriedItemUsefulForOrder(actor, order))
                    {
                        // Use precomputed allocation from TryCollectIngredients
                        var allocation = allocations
                            .SelectMany(a => a.pair)
                            .FirstOrDefault(a => a.stack == carried);

                        // If an allocation is found and valid, issue a go pick up plan
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
                if (nextItem != null)
                {
                    // If the target workstation slots are occupied, try to clear them
                    if (TryClearWorkstations(actor, manager) is Entity junk)
                        return new Plan(PlanDefOf.GoHaul, new TargetArgs(junk));
                    return new Plan(PlanDefOf.GoHaul, new TargetArgs(nextItem.Value.stack)) { AmountA = nextItem.Value.quantity };
                }
            }

            return null;
        }
        static Entity TryClearWorkstations(Actor actor, CraftingManager manager)
        {
            foreach (var workstation in manager.AllWorkstations)
                foreach (var junk in workstation.GetJunk().Where(j => j is not Actor))
                    if (actor.CanReachAndReserve(junk))
                        return junk;
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
                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);
                int slotQuantity = slotEntities.Sum(e => req.MatchesPartial(e, out var q) ? q : 0);

                if (req.Matches(carried) && 
                    carried.StackSize + slotQuantity >= req.Quantity &&
                    actor.CanReachAndReserve(req.Slot)
                    )
                {
                    targetSlot = req.Slot;
                    return true;
                }
            }

            return false;
        }
        bool IsCarriedItemUsefulForOrder(Actor actor, OrderSettings order)
        {
            if (actor.Hauled is not Entity carried)
                return false;

            foreach (var req in order.GetIngredientRequirements())
            {
                if (!req.Matches(carried))
                    continue;

                int demandForCarried = req.Quantity;

                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);
                foreach (var e in slotEntities)
                    if (req.MatchesPartial(e, out var used))
                        demandForCarried -= used;

                if (demandForCarried > 0)
                    return true;
            }
            return false;
        }
       

        bool IsCarriedItemRelevantForAnyOrder(Actor actor, IEnumerable<OrderSettings> orders)
        {
            var carried = actor.Hauled as Entity;
            if (carried == null)
                return false;
            foreach (var order in orders)// actor.Map.Town.CraftingManagerNew.GetAllOrdersUnsorted())
                foreach (var req in order.GetIngredientRequirements())
                    if (req.Matches(carried))
                        return true;

            return false;
        }
        private static bool AllReagentsAvailable(GameObject actor, List<GameObject> allObjects, ref List<Dictionary<TargetArgs, int>> itemAmounts, Dictionary<string, int> materialsUsed, CraftOrder order)
        {
            return AllReagentsAvailable(actor, allObjects, ref itemAmounts, materialsUsed, order);
        }
        
        enum CraftingOrderStateOld
        {
            NotEnoughItems,      // No ingredients available at all
            NeedsTransfer,       // Ingredients exist on the map but not in slots
            ReadyToCraft         // All required ingredients are already in slots
        }
        struct CraftingCollectionResult
        {
            public CraftingOrderStateOld State;  // NotEnoughItems, NeedsTransfer, ReadyToCraft
            public IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> ToTransfer; // map items to move to slots
            public IEnumerable<(IntVec3 slot, Entity entity)> InSlots;       // items already in slots

            public CraftingCollectionResult(CraftingOrderStateOld state, IEnumerable<(IEnumerable<(Entity stack, int quantity)> pair, IntVec3 slot)> toTransfer, IEnumerable<(IntVec3 slot, Entity entity)> inSlots)
            {
                State = state;
                ToTransfer = toTransfer;
                InSlots = inSlots;
            }
        }
        private static CraftingCollectionResult TryCollectIngredientsNewRules(Actor actor, OrderSettings order)
        {
            var mapItems = actor.Map.GetEntities<Entity>().Where(actor.CanReachAndReserve);
            Dictionary<Entity, int> allocatedSoFar = [];
            List<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)> allFound = [];
            List<(IntVec3 slot, Entity entity)> inSlots = [];
            var ingredients = order.GetIngredientRequirements().ToList();
            foreach(var item in mapItems)
            {

            }
            foreach (var req in ingredients)
            {
                var missingQuantity = req.Quantity;
                if (req.InSlot.FirstOrDefault(
                    entity =>
                        req.Matches(entity) &&
                        req.Quantity == entity.StackSize &&
                        actor.CanReachAndReserve(entity))
                    is Entity inSlot)
                {
                    inSlots.Add((req.Slot, inSlot));
                    continue;
                }
                if (!actor.CanReachAndReserve(req.Slot))
                    break;
                if (missingQuantity > 0 && actor.Hauled is Entity carried && req.Matches(carried))
                {
                    var used = Math.Min(carried.StackSize, missingQuantity);
                    missingQuantity -= used;
                }

                Debug.Assert(missingQuantity >= 0);
                if (missingQuantity == 0)
                    continue;
                var validStacks = mapItems.Where(req.Matches);
                var allocation = AllocateRequirement(actor, validStacks, missingQuantity, allocatedSoFar);
                if (allocation is null)
                    return new CraftingCollectionResult(CraftingOrderStateOld.NotEnoughItems, null, null);
                allFound.Add((allocation, req.Slot));
            }
            if (inSlots.Count == ingredients.Count && allFound.Count != 0)
                throw new Exception("nothing else should be returned as found if slots are already fulfilled");
            if (inSlots.Count == ingredients.Count)
                return new(CraftingOrderStateOld.ReadyToCraft, null, inSlots);

            return new(CraftingOrderStateOld.NeedsTransfer, allFound, null);
        }
        bool IsFeasible(Actor actor, OrderSettings order)
        {
            if (order.IsReadyToCraft(out _))
                return true;
            var alreadyBound = order.AlreadyBoundInSlots();
            var mapItems = actor.Map.GetEntities<Entity>().Where(actor.CanReachAndReserve)
                .Except(alreadyBound)
                .Prepend(actor.Hauled as Entity);
            foreach(var item in mapItems)
            {

            }
            return false;
        }
        
        private static CraftingCollectionResult TryCollectIngredientsNew(Actor actor, OrderSettings order)
        {
            var mapItems = actor.Map.GetEntities<Entity>().Where(actor.CanReachAndReserve);
            Dictionary<Entity, int> allocatedSoFar = [];
            List<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)> allFound = [];
            List<(IntVec3 slot, Entity entity)> inSlots = [];
            var ingredients = order.GetIngredientRequirements().ToList();
            foreach (var req in order.GetIngredientRequirements())
            {
                var missingQuantity = req.Quantity;
                if (req.InSlot.FirstOrDefault(
                    entity =>
                        req.Matches(entity) &&
                        req.Quantity == entity.StackSize &&
                        actor.CanReachAndReserve(entity))
                    is Entity inSlot)
                {
                    inSlots.Add((req.Slot, inSlot));
                    continue;
                }
                if (!actor.CanReachAndReserve(req.Slot))
                    break;
                if (missingQuantity > 0 && actor.Hauled is Entity carried && req.Matches(carried))
                {
                    var used = Math.Min(carried.StackSize, missingQuantity);
                    missingQuantity -= used;
                }

                Debug.Assert(missingQuantity >= 0);
                if (missingQuantity == 0)
                    continue;
                var validStacks = mapItems.Where(req.Matches);
                var allocation = AllocateRequirement(actor, validStacks, missingQuantity, allocatedSoFar);
                if (allocation is null)
                    return new CraftingCollectionResult(CraftingOrderStateOld.NotEnoughItems, null, null);
                allFound.Add((allocation, req.Slot));
            }
            if (inSlots.Count == ingredients.Count && allFound.Count != 0)
                throw new Exception("nothing else should be returned as found if slots are already fulfilled");
            if (inSlots.Count == ingredients.Count)
                return new(CraftingOrderStateOld.ReadyToCraft, null, inSlots);
            
            return new(CraftingOrderStateOld.NeedsTransfer, allFound, null);
        }
        private static CraftingCollectionResult TryCollectIngredients(Actor actor, OrderSettings order)
        {
            var mapEntities = actor.Map.GetEntities<Entity>().Where(actor.CanReachAndReserve);
            Dictionary<Entity, int> allocatedSoFar = [];
            List<(IEnumerable<(Entity stack, int quantity)>, IntVec3 slot)> allFound = [];
            List<(IntVec3 slot, Entity entity)> inSlots = [];
          
            foreach (var req in order.GetIngredientRequirements())
            {
                var missingQuantity = req.Quantity;
                var slotEntities = order.Workstation.Map.GetEntitiesAt(req.Slot);

                //if (slotEntities.Any(entity => req.Matches(entity) && req.Quantity == entity.StackSize))
                if (slotEntities.FirstOrDefault(
                    entity => 
                        req.Matches(entity) && 
                        req.Quantity == entity.StackSize &&
                        actor.CanReachAndReserve(entity)) 
                    is Entity inSlot)
                {
                    inSlots.Add((req.Slot, inSlot));
                    break;
                }
                if (missingQuantity > 0 && actor.Hauled is Entity carried && req.Matches(carried))
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
                    return new CraftingCollectionResult(CraftingOrderStateOld.NotEnoughItems, null, null);
                allFound.Add((allocation, req.Slot));
            }
            if (allFound.Count == 0)
                return new(CraftingOrderStateOld.ReadyToCraft, null, inSlots);
            return new(CraftingOrderStateOld.NeedsTransfer, allFound, null);
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
