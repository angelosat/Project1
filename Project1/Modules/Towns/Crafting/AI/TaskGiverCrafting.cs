using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverCrafting : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
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
                    var operatingPos = map.GetCell(benchglobal).GetInteractionSpots(benchglobal).First();
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
                        var task = new AITask(TaskDefOf.Crafting);
                        task.Order = order;
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
                        var task = new AITask(TaskDefOf.Crafting);
                        foreach (var dic in itemAmounts)
                            foreach (var itemAmount in dic)
                                task.AddTarget(TaskBehaviorCrafting.IngredientIndex, itemAmount.Key, itemAmount.Value);
                        task.SetTarget(TaskBehaviorCrafting.WorkstationIndex, workstationTarget);
                        task.Order = order;
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
                                   select stack).OrderByReachableRegionDistance(actor); // closest to actor or workstation?
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
