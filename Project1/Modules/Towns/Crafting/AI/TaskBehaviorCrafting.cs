using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Crafting;
using Start_a_Town_.Net;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskBehaviorCrafting : BehaviorPerformTask
    {
        TargetArgs Workstation => this.Task.GetTarget(WorkstationIndex);
        public static TargetIndex AuxiliaryIndex = TargetIndex.C;
        public static TargetIndex IngredientIndex = TargetIndex.B;
        public static TargetIndex WorkstationIndex = TargetIndex.A;

        protected override IEnumerable<Behavior> GetSteps()
        {
            var workstationGlobal = this.Workstation.Global; // capture this here because later i replace the workstation's target index with the product
            var actor = this.Actor;
            /// TODO constantly check for operating positions? or let pathing fail when no free operating position found?
            //this.FailOn(noOperatingPositions);
            var task = this.Task;
            var order = task.Order;

            var nextIngredient = BehaviorHelper.ExtractNextTargetAmount(IngredientIndex);
            yield return nextIngredient;
            yield return new BehaviorGetAtNewNew(IngredientIndex).FailOnUnavailableTarget(IngredientIndex).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHaulHelper.StartCarrying(IngredientIndex)
                .FailOn(failOnInvalidWorkstation)
                .FailOn(orderIncompletable)
                .FailOn(noOperatingPositions);
            yield return BehaviorHelper.JumpIfNextCarryStackable(nextIngredient, IngredientIndex);

            yield return new BehaviorCustom()
            {
                InitAction = () =>
                    this.Task.SetTarget(AuxiliaryIndex, new TargetArgs(this.Actor.Map, this.Workstation.Global.Above()))
            };
            yield return new BehaviorGetAtNewNew(WorkstationIndex, PathEndMode.InteractionSpot).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return new BehaviorInteractionNew(AuxiliaryIndex, () => new UseHauledOnTarget()).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHelper.JumpIfMoreTargets(nextIngredient, IngredientIndex);
            ///NO!!!! if they collected more than one item in the same stack, this code will only add the last (disposed) target that was merged to the stack
            ///add code in usehauledontarget interaction instead

            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(WorkstationIndex, PathEndMode.InteractionSpot).FailOn(placedObjectsChanged).FailOn(orderIncompletable);

            /// create unfinished item
            yield return new BehaviorCustom(delegate
            {
                if (order.UnfinishedItem is not null)
                {
                    this.Task.SetTarget(AuxiliaryIndex, order.UnfinishedItem);
                    return;
                }
                if (!order.Reaction.CreatesUnfinishedItem)
                    return;

                //var ingr = this.Task.PlacedObjects.Select(o => new ObjectAmount(actor.Net.GetNetworkObject(o.Object), o.Amount)).ToList();
                var product = order.Reaction.Products.First().Make(actor, order.Reaction, this.Task.PlacedObjects);
                //product.SyncConsumeMaterials(actor.Net);
                foreach (var o in product.RequirementsNew.Values.Select(o => o.Object).Distinct())
                    //PacketEntityRequestDispose.Send(actor.Net, o.RefID);
                    o.SyncDispose();
                var item = ItemDefOf.UnfinishedCraft.Create();
                item.GetComponent<UnfinishedItemComp>().SetProduct(product, actor, order);
                item.SyncInstantiate(actor.Net as NetEndpoint);

                actor.Map.SyncSpawn(item, this.Workstation.Global.Above(), IntVec3.Zero);

                task.SetTarget(AuxiliaryIndex, item);
                actor.Reserve(task, item);
            });

            yield return new BehaviorInteractionNew(WorkstationIndex, () => new InteractionCrafting(task.Order, task.PlacedObjects, task.GetTarget(AuxiliaryIndex).Object as Entity)).FailOn(placedObjectsChanged).FailOn(orderIncompletable);
            if (this.Task.Tool?.Type != TargetType.Null) // dont unequip tool if not using any
                yield return new BehaviorInteractionNew(TargetIndex.Tool, () => new InteractionEquip()); // unequip the tool before hauling product // TODO dont do that if no tool equipped
            // assign a new haul behavior directly to the actor instead of adding the steps here?
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var haulamount = this.Task.Product.Object.StackSize;
                    var product = this.Task.Product.Object;
                    var productTar = this.Task.Product;
                    var order = this.Task.Order;
                    this.Actor.Reserve(this.Task, productTar, haulamount); // was using -1 to denote full stack, but want to phase it out
                    if (order.Output is Stockpile stockpile && stockpile.GetPotentialHaulTargets(actor, product) is var places && places.Any())// ; Towns.StockpileManager.GetBestStoragePlace(this.Actor, this.Task.Product.Object as Entity, out TargetArgs target))
                    {
                        var target = places.First();
                        this.Task.SetTarget(WorkstationIndex, target);
                        this.Task.SetTarget(IngredientIndex, productTar, haulamount);
                    }
                    else if (HaulHelper.TryFindNearbyPlace(actor, product, this.Task.GetTarget(WorkstationIndex).Global, out TargetArgs nearby))
                    {
                        this.Task.SetTarget(WorkstationIndex, nearby);
                        this.Task.SetTarget(IngredientIndex, productTar, haulamount);
                    }
                    else
                        this.Actor.EndCurrentTask();
                }
            };

            yield return new BehaviorGetAtNewNew(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return BehaviorHaulHelper.StartCarrying(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return new BehaviorGetAtNewNew(WorkstationIndex).FailOn(deliverFail);
            yield return BehaviorHelper.PlaceCarried(WorkstationIndex).FailOn(deliverFail);

            bool orderIncompletable()
            {
                var val = !this.Task.Order.IsActive || !this.Task.Order.IsCompletable();
                if (val)
                    "order incompletable".ToConsole();
                return val;
            }
            bool failOnInvalidWorkstation()
            {
                return !(this.Workstation.BlockEntity?.HasComp<BlockEntityCompWorkstation>() ?? false);
            };
            bool deliverFail()
            {
                if (this.Task.Order.HaulOnFinish)
                    return !HaulHelper.IsValidStorage(this.Task.GetTarget(WorkstationIndex), this.Actor.Map, this.Task.Product.Object);
                else
                    return !this.IsValidStorage(this.Task.GetTarget(WorkstationIndex));
            };
            bool noOperatingPositions()
            {
                /// TODO constantly check for operating positions? or let pathing fail when no free operating position found?
                var map = this.Actor.Map;
                var front = map.GetFrontOfBlock(workstationGlobal);
                if (!map.IsStandableIn(front))
                {
                    "no operating position available".ToConsole();
                    return true;
                }
                return false;
            };
            bool placedObjectsChanged()
            {
                if (this.Task.TargetC?.Object is Entity unf && unf.Def == ItemDefOf.UnfinishedCraft)
                {
                    if (unf.IsDisposed || unf.IsForbidden || unf.CellIfSpawned != this.Workstation.Global.Above())
                        return true;
                    else
                        return false;
                }

                foreach (var t in this.Task.PlacedObjects)
                {
                    if (t.Object.IsDisposed)
                        return true;
                    if (t.Object.IsForbidden)
                        return true;
                    if (t.Object.CellIfSpawned != this.Workstation.Global.Above())
                        return true;
                }
                return false;
            }
        }
        protected override bool InitExtraReservations()
        {
            var task = this.Task;
            var actor = this.Actor;
            var benchGlobal = (IntVec3)this.Workstation.Global;
            var benchGlobalAbove = benchGlobal.Above;
            var operatingPos = actor.Map.GetFrontOfBlock(benchGlobal);
            var operatingPosBelow = operatingPos.Below;
            return task.ReserveAll(actor, IngredientIndex)
                && actor.Reserve(task, benchGlobal)
                && actor.Reserve(task, benchGlobalAbove)
                && actor.Reserve(task, operatingPos)
                && actor.Reserve(task, operatingPosBelow);
        }
        bool IsValidStorage(TargetArgs target)
        {
            if (target.HasObject)
            {
                return !target.Object.IsForbidden && target.Object.CanAbsorb(this.Task.Product.Object);
            }
            else
            {
                var map = this.Actor.Map;
                var global = target.Global;
                return map.IsSolid(global.Below()) && map.IsEmpty(global);
            }
        }
    }
}
