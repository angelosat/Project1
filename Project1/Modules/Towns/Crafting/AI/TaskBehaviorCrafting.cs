using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Crafting;
using Start_a_Town_.Net;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class TaskBehaviorCrafting : BehaviorExecutePlan
    {
        TargetArgs Workstation => this.Plan.GetTarget(WorkstationIndex);
        public static TargetIndex AuxiliaryIndex = TargetIndex.C;
        public static TargetIndex IngredientIndex = TargetIndex.B;
        public static TargetIndex WorkstationIndex = TargetIndex.A;

        protected override IEnumerable<Behavior> GetSteps()
        {
            var workstationGlobal = this.Workstation.Global; // capture this here because later i replace the workstation's target index with the product
            var actor = this.Actor;
            /// TODO constantly check for operating positions? or let pathing fail when no free operating position found?
            //this.FailOn(noOperatingPositions);
            var task = this.Plan;
            var order = task.OrderOld;

            var nextIngredient = BehaviorHelper.ExtractNextTargetAmount(IngredientIndex);
            yield return nextIngredient;
            yield return new BehaviorResolvePath(IngredientIndex).FailOnUnavailableTarget(IngredientIndex).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHaulHelper.StartCarrying(this, IngredientIndex)
                .FailOn(failOnInvalidWorkstation)
                .FailOn(orderIncompletable)
                .FailOn(noOperatingPositions);
            yield return BehaviorHelper.JumpIfNextCarryStackable(nextIngredient, IngredientIndex);

            yield return new BehaviorCustom()
            {
                InitAction = () =>
                    this.Plan.SetTarget(AuxiliaryIndex, new TargetArgs(this.Actor.Map, this.Workstation.Global.Above()))
            };
            yield return new BehaviorResolvePath(WorkstationIndex, PathEndMode.InteractionSpot).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return new BehaviorResolveInteraction(AuxiliaryIndex, () => new UseHauledOnTarget()).FailOn(failOnInvalidWorkstation).FailOn(orderIncompletable).FailOn(noOperatingPositions);
            yield return BehaviorHelper.JumpIfMoreTargets(nextIngredient, IngredientIndex);
            ///NO!!!! if they collected more than one item in the same stack, this code will only add the last (disposed) target that was merged to the stack
            ///add code in usehauledontarget interaction instead

            yield return new BehaviorGrabTool();
            yield return new BehaviorResolvePath(WorkstationIndex, PathEndMode.InteractionSpot).FailOn(placedObjectsChanged).FailOn(orderIncompletable);

            /// create unfinished item
            yield return new BehaviorCustom(delegate
            {
                if (order.UnfinishedItem is not null)
                {
                    this.Plan.SetTarget(AuxiliaryIndex, order.UnfinishedItem);
                    return;
                }
                if (!order.Reaction.CreatesUnfinishedItem)
                    return;

                //var ingr = this.Task.PlacedObjects.Select(o => new ObjectAmount(actor.Net.GetNetworkObject(o.Object), o.Amount)).ToList();
                var product = order.Reaction.Products.First().Make(actor, order.Reaction, this.Plan.PlacedObjects);
                //product.SyncConsumeMaterials(actor.Net);
                foreach (var o in product.RequirementsNew.Values.Select(o => o.Object).Distinct())
                    //PacketEntityRequestDispose.Send(actor.Net, o.RefID);
                    //o.SyncDispose();
                    actor.Map.World.DisposeEntityAndSync(o as Entity);
                var item = ItemDefOf.UnfinishedCraft.Create();
                item.GetComponent<UnfinishedItemComp>().SetProduct(product, actor, order);
                item.SyncInstantiate(actor.Net as NetEndpoint);

                //actor.Map.SyncSpawn(item, this.Workstation.Global.Above(), IntVec3.Zero);
                actor.Map.SpawnAndSync(item, this.Workstation.Global.Above(), IntVec3.Zero);

                task.SetTarget(AuxiliaryIndex, item);
                //actor.Reserve(task, item);
                this.Reserve(item);
            });

            yield return new BehaviorResolveInteraction(WorkstationIndex, () => new InteractionCrafting(task.OrderOld, task.PlacedObjects, task.GetTarget(AuxiliaryIndex).Object as Entity)).FailOn(placedObjectsChanged).FailOn(orderIncompletable);
            if (this.Plan.Tool?.Type != TargetType.Null) // dont unequip tool if not using any
                yield return new BehaviorResolveInteraction(TargetIndex.Tool, () => new InteractionEquip()); // unequip the tool before hauling product // TODO dont do that if no tool equipped
            // assign a new haul behavior directly to the actor instead of adding the steps here?
            yield return new BehaviorCustom()
            {
                InitAction = () =>
                {
                    var haulamount = this.Plan.Product.Object.StackSize;
                    var product = this.Plan.Product.Object;
                    var productTar = this.Plan.Product;
                    var order = this.Plan.OrderOld;
                    //this.Actor.Reserve(this.Task, productTar, haulamount); // was using -1 to denote full stack, but want to phase it out
                    this.Reserve(productTar, haulamount); // was using -1 to denote full stack, but want to phase it out
                    if (order.Output is Stockpile stockpile && stockpile.GetPotentialHaulTargets(actor, product) is var places && places.Any())// ; Towns.StockpileManager.GetBestStoragePlace(this.Actor, this.Task.Product.Object as Entity, out TargetArgs target))
                    {
                        var target = places.First();
                        this.Plan.SetTarget(WorkstationIndex, target);
                        this.Plan.SetTarget(IngredientIndex, productTar, haulamount);
                    }
                    else if (HaulHelper.TryFindNearbyPlace(actor, product, this.Plan.GetTarget(WorkstationIndex).Global, out TargetArgs nearby))
                    {
                        this.Plan.SetTarget(WorkstationIndex, nearby);
                        this.Plan.SetTarget(IngredientIndex, productTar, haulamount);
                    }
                    else
                        this.Actor.EndCurrentTask();
                }
            };

            yield return new BehaviorResolvePath(IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return BehaviorHaulHelper.StartCarrying(this, IngredientIndex).FailOn(deliverFail).FailOnUnavailableTarget(IngredientIndex);
            yield return new BehaviorResolvePath(WorkstationIndex).FailOn(deliverFail);
            yield return BehaviorHelper.PlaceCarried(WorkstationIndex).FailOn(deliverFail);

            bool orderIncompletable()
            {
                var val = !this.Plan.OrderOld.IsActive || !this.Plan.OrderOld.IsCompletable();
                if (val)
                    "order incompletable".ToConsole();
                return val;
            }
            bool failOnInvalidWorkstation()
            {
                return !(this.Workstation.BlockEntityOld?.HasComp<BlockEntityCompWorkstationOld>() ?? false);
            };
            bool deliverFail()
            {
                if (this.Plan.OrderOld.HaulOnFinish)
                    return !HaulHelper.IsValidStorage(this.Plan.GetTarget(WorkstationIndex), this.Actor.Map, this.Plan.Product.Object);
                else
                    return !this.IsValidStorage(this.Plan.GetTarget(WorkstationIndex));
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
                if (this.Plan.TargetC?.Object is Entity unf && unf.Def == ItemDefOf.UnfinishedCraft)
                {
                    if (unf.IsDisposed || unf.IsForbidden || unf.CellIfSpawned != this.Workstation.Global.Above())
                        return true;
                    else
                        return false;
                }

                foreach (var t in this.Plan.PlacedObjects)
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
            var task = this.Plan;
            var actor = this.Actor;
            var benchGlobal = (IntVec3)this.Workstation.Global;
            var benchGlobalAbove = benchGlobal.Above;
            var operatingPos = actor.Map.GetFrontOfBlock(benchGlobal);
            var operatingPosBelow = operatingPos.Below;
            return this.ReserveAll(IngredientIndex)
                //&& actor.Reserve(task, benchGlobal)
                //&& actor.Reserve(task, benchGlobalAbove)
                //&& actor.Reserve(task, operatingPos)
                //&& actor.Reserve(task, operatingPosBelow);
                && this.Reserve(benchGlobal)
                && this.Reserve(benchGlobalAbove)
                && this.Reserve(operatingPos)
                && this.Reserve(operatingPosBelow);
        }
        bool IsValidStorage(TargetArgs target)
        {
            if (target.HasObject)
            {
                return !target.Object.IsForbidden && target.Object.CanAbsorb(this.Plan.Product.Object);
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
