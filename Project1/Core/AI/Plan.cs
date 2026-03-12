using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Planners;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using Project1.Core.Legacy;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Towns;
using Project1.Core.Towns.Designations;
using Project1.Core.Towns.Shops;
using Project1.Core.Towns.Zones;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Project1.Core.AI
{
    public enum PlanContinuationPolicy { Continue, Yield }
    public enum TargetIndex { None, A, B, C }
    public sealed class Plan
    {
        public PlanContinuationPolicy Continuation;
        public TargetArgs TargetA = TargetArgs.Null;
        public TargetArgs TargetB = TargetArgs.Null;
        public TargetArgs TargetC = TargetArgs.Null;
        public List<TargetArgs> TargetsA = [];
        public List<TargetArgs> TargetsB = [];
        public List<TargetArgs> TargetsC = [];
        public List<int> AmountsA = [];
        public List<int> AmountsB = [];
        public List<int> AmountsC = [];
        public int AmountA = -1, AmountB = -1, AmountC = -1;
        public int Count;
        public List<List<TargetArgs>> TargetQueues = [];
        public List<List<int>> AmountQueues = [];
        public List<ObjectAmount> PlacedObjects = [];
        public List<Entity> CraftedItems = [];
        public DesignationDef Designation;
        public CraftingOrder Order;
        public TargetArgs Product = TargetArgs.Null;
        public bool Forced;
        public bool Urgent = true; // TODO default should be false
        int ReservedBy = -1;
        public int TicksWaited = 0;
        public int TicksTimeout;
        public int TicksCounter;
        public int Quest;
        public int ShopID; // TODO store shopid instead of shop object
        public Transaction Transaction;
        public CustomerProperties CustomerProps;
        public int CustomerID;
        bool Cancelled = false;
        ZoneId _zoneID = ZoneId.Null;
        Zone _zone;
        public PlannerDef Source { get; init; }
        public Zone? Zone
        {
            get => this._zoneID == ZoneId.Null ? null : this._zone ??= this.Actor.Map.Town.ZoneManager.GetZone(this._zoneID);
            set
            {
                if (value is not null)
                {
                    this._zoneID = value.ID;
                    this._zone = value;
                }
            }
        }
        public bool ZoneRequired => this._zoneID != ZoneId.Null;
        public bool DesignationRequired => this.Designation is not null;
        public bool IsCancelled => this.Cancelled;
        Type _BehaviorType;
        internal bool IsUrgent;
        internal Actor Actor;
        //public bool IsReserved => this.ReservedBy > -1;
        InteractionDef EndGoal;
        Func<bool> _evaluator;
        Func<bool> Evaluator => _evaluator ??= () =>
                {
                    var ctx = this.EndGoal.CreateContext(this.Actor, this.TargetA, this.AmountA);
                    return this.EndGoal.Logic.CanPerform(ctx);
                };
        internal bool IsEndGoalFeasible() => this.Evaluator();
        public PlanDef Def;
        //public int ID { get; internal set; }
        public string Status => $"{this.Def.Interaction?.LabelReadable} : {this.TargetA}";
        public TargetArgs GetTarget(TargetIndex targetInd)
        {
            return targetInd switch
            {
                TargetIndex.A => this.TargetA,
                TargetIndex.B => this.TargetB,
                TargetIndex.C => this.TargetC,
                _ => throw new Exception(),
            };
        }
        public bool IsImmediate = true;
        internal TargetArgs GetTarget(int targetInd)
        {
            return this.GetTarget((TargetIndex)targetInd);
        }
        internal int GetAmount(TargetIndex amountInd)
        {
            return amountInd switch
            {
                TargetIndex.A => this.AmountA,
                TargetIndex.B => this.AmountB,
                TargetIndex.C => this.AmountC,
                _ => throw new Exception(),
            };
        }
        internal List<TargetArgs> GetTargetQueue(TargetIndex targetInd)
        {
            return targetInd switch
            {
                TargetIndex.A => this.TargetsA,
                TargetIndex.B => this.TargetsB,
                TargetIndex.C => this.TargetsC,
                _ => throw new Exception(),
            };
        }
        internal List<int> GetAmountQueue(TargetIndex amountInd)
        {
            return amountInd switch
            {
                TargetIndex.A => this.AmountsA,
                TargetIndex.B => this.AmountsB,
                TargetIndex.C => this.AmountsC,
                _ => throw new Exception(),
            };
        }
        internal Plan SetTarget(TargetIndex targetInd, Entity target, int amount)
        {
            this.SetAmount(targetInd, amount);
            return this.SetTarget(targetInd, new TargetArgs(target));
        }
        internal Plan SetTarget(TargetIndex targetInd, TargetArgs targetArgs)
        {
            switch (targetInd)
            {
                case TargetIndex.A:
                    this.TargetA = targetArgs;
                    break;
                case TargetIndex.B:
                    this.TargetB = targetArgs;
                    break;
                case TargetIndex.C:
                    this.TargetC = targetArgs;
                    break;
                default:
                    throw new Exception();
            }
            return this;
        }

        internal void AddPlacedObject(Entity hauledObj)
        {
            this.PlacedObjects.Add(new ObjectAmount(hauledObj));
        }
        internal void AddCraftedItem(Entity item)
        {
            this.CraftedItems.Add(item);
        }
        internal void SetAmount(TargetIndex ind, int amount)
        {
            switch (ind)
            {
                case TargetIndex.A:
                    this.AmountA = amount;
                    break;
                case TargetIndex.B:
                    this.AmountB = amount;
                    break;
                case TargetIndex.C:
                    this.AmountC = amount;
                    break;
                default:
                    throw new Exception();
            }
        }
        internal bool NextTarget(TargetIndex ind)
        {
            var targets = this.GetTargetQueue(ind);
            if (!targets.Any())
                return false;
            this.SetTarget(ind, targets[0]);
            targets.RemoveAt(0);
            return true;
        }
        internal bool NextAmount(TargetIndex ind)
        {
            var targets = this.GetAmountQueue(ind);
            if (!targets.Any())
                return false;
            this.SetAmount(ind, targets[0]);
            targets.RemoveAt(0);
            return true;
        }
        public static Plan Load(SaveTag tag)
        {
            var task = new Plan();
            task.LoadData(tag);
            return task;
        }
        public Plan()
        {
        }
        public Plan(PlanDef taskDef)
        {
            if (taskDef is null) throw new Exception();
            this.Def = taskDef;
        }
        public Plan(Type behaviorType) : this()
        {
            throw new Exception();

            this.BehaviorType = behaviorType;
        }
        [Obsolete("use a ctor which accepts a plandef")]
        public Plan(Type behaviorType, TargetArgs targetA) : this()
        {
            throw new Exception();
            this.BehaviorType = behaviorType;
            this.SetTarget(TargetIndex.A, targetA);
        }
        public Plan(PlanDef def, TargetArgs interactionTarget) : this()
        {
            ArgumentNullException.ThrowIfNull(def);

            this.Def = def;
            this.SetTarget(TargetIndex.A, interactionTarget);
        }
        public Plan(PlanDef def, MapBase map, IntVec3 pos, int amount) : this()
        {
            ArgumentNullException.ThrowIfNull(def);

            this.Def = def;
            this.SetTarget(TargetIndex.A, new TargetArgs(map, pos));
            this.SetAmount(TargetIndex.A, amount);
        }
        public Plan(PlanDef def, Entity item, int amount = -1) : this()
        {
            ArgumentNullException.ThrowIfNull(def);

            this.Def = def;
            this.SetTarget(TargetIndex.A, new TargetArgs(item));
            this.SetAmount(TargetIndex.A, amount);
        }
        public Plan(PlanDef def, TargetArgs targetA, TargetArgs targetB) : this()
        {
            if (def is null) throw new Exception();

            this.Def = def;
            this.SetTarget(TargetIndex.A, targetA);
            this.SetTarget(TargetIndex.B, targetB);
        }
        public Plan(Type behaviorType, TargetArgs targetA, TargetArgs targetB) : this()
        {
            throw new Exception();
            this.BehaviorType = behaviorType;
            this.SetTarget(TargetIndex.A, targetA);
            this.SetTarget(TargetIndex.B, targetB);
        }

        public override string ToString()
        {
            return this.Def?.Name ?? base.ToString();
        }
        internal void Cancel()
        {
            this.Cancelled = true;
        }

        public void Reserve(GameObject actor)
        {
            this.ReservedBy = actor.RefId;
        }

        public string GetForceTaskText()
        {
            return this.Def?.GetForceText(this) ?? this.BehaviorType.Name;
        }

        public Type BehaviorType
        {
            get => this.Def?.BehaviorClass ?? this._BehaviorType;
            set => this._BehaviorType = value;
        }

        public PlanExecutor CreateBehavior(Actor actor)
        {
            var behav = ActivatorSafe<PlanExecutor>.CreateInstance(this.BehaviorType);
            behav.Actor = actor;
            behav.Plan = this;
            return behav;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            tag.Add(this.TargetA.Save("TargetA"));
            tag.Add(this.TargetB.Save("TargetB"));
            tag.Add(this.TargetC.Save("TargetC"));

            tag.Add(this.AmountA.Save("AmountA"));
            tag.Add(this.AmountB.Save("AmountB"));
            tag.Add(this.AmountC.Save("AmountC"));

            tag.Add(this.TargetsA.Save("TargetsA"));
            tag.Add(this.TargetsB.Save("TargetsB"));
            tag.Add(this.TargetsC.Save("TargetsC"));

            tag.Add(this.AmountsA.Save("AmountsA"));
            tag.Add(this.AmountsB.Save("AmountsB"));
            tag.Add(this.AmountsC.Save("AmountsC"));

            tag.Add(this.Count.Save("Count"));
            tag.Add(this.Product.Save("Product"));
            tag.Add(this.Forced.Save("Forced"));

            this.TicksWaited.Save(tag, "TicksWaited");
            this.TicksCounter.Save(tag, "TicksCounter");
            this.TicksTimeout.Save(tag, "TicksTimeout");

            var targetqueues = new SaveTag(SaveTag.Types.List, "Queues", SaveTag.Types.List);
            for (int i = 0; i < this.TargetQueues.Count; i++)
            {
                var tarqueue = this.TargetQueues[i];
                var quantityqueue = this.AmountQueues[i];
                var queuetag = new SaveTag(SaveTag.Types.List, "", SaveTag.Types.Compound);
                for (int j = 0; j < tarqueue.Count; j++)
                {
                    var itemtag = new SaveTag(SaveTag.Types.Compound);
                    var tar = tarqueue[j];
                    var amount = quantityqueue[j];
                    itemtag.Add(tar.Save("Target"));
                    itemtag.Add(amount.Save("Amount"));
                    queuetag.Add(itemtag);
                }
                targetqueues.Add(queuetag);
            }
            tag.Add(targetqueues);

            this.ShopID.Save(tag, "ShopID");
            this.Quest.Save(tag, "QuestToAccept");
            this.Transaction.Save(tag, "Transaction");

            tag.Save("Continuation", (int)this.Continuation);

            if(this.Designation is not null)
                tag.Save("Designation", this.Designation);
            if(this._zoneID != ZoneId.Null)
                tag.Save("ZoneID", this._zoneID);

            this.AddSaveData(tag);
            return tag;
        }
        protected void AddSaveData(SaveTag tag)
        {

        }
        public void LoadData(SaveTag tag)
        {
            this.Def = tag.LoadDef<PlanDef>("Def");
            tag.TryGetTag("TargetA", t => this.TargetA = new TargetArgs(t));
            tag.TryGetTag("TargetB", t => this.TargetB = new TargetArgs(t));
            tag.TryGetTag("TargetC", t => this.TargetC = new TargetArgs(t));


            tag.TryGetTagValueOrDefault("AmountA", out this.AmountA);
            tag.TryGetTagValueOrDefault("AmountB", out this.AmountB);
            tag.TryGetTagValueOrDefault("AmountC", out this.AmountC);

            tag.TryGetTag("TargetsA", t => this.TargetsA.Load(t));
            tag.TryGetTag("TargetsB", t => this.TargetsB.Load(t));
            tag.TryGetTag("TargetsC", t => this.TargetsC.Load(t));

            tag.TryGetTag("AmountsA", t => this.AmountsA.Load(t));
            tag.TryGetTag("AmountsB", t => this.AmountsB.Load(t));
            tag.TryGetTag("AmountsC", t => this.AmountsC.Load(t));

            tag.TryGetTagValueOrDefault("TicksCounter", out this.TicksCounter);
            tag.TryGetTagValueOrDefault("TicksWaited", out this.TicksWaited);
            tag.TryGetTagValueOrDefault("TicksTimeout", out this.TicksTimeout);


            tag.TryGetTagValueOrDefault("Count", out this.Count);
            tag.TryGetTag("Product", t => this.Product = new TargetArgs(t));
            tag.TryGetTagValueOrDefault("Forced", out this.Forced);
            if (tag.TryGetTagValueOrDefault("Queues", out List<SaveTag> queuestag))
            {
                foreach (var qtag in queuestag)
                {
                    var curqtag = qtag.Value as List<SaveTag>;
                    var tlist = new List<TargetArgs>();
                    var clist = new List<int>();
                    foreach (var ctag in curqtag)
                    {
                        var tar = new TargetArgs(ctag["Target"]);

                        var amount = (int)ctag["Amount"].Value;
                        tlist.Add(tar);
                        clist.Add(amount);
                    }
                    this.TargetQueues.Add(tlist);
                    this.AmountQueues.Add(clist);
                }
            }
            tag.TryGetTagValueOrDefault("ShopID", out this.ShopID);
            tag.TryGetTagValueOrDefault("QuestToAccept", out this.Quest);
            tag.TryGetTag("Transaction", v => this.Transaction = new Transaction(v));

            tag.TryGetTagValue<int>("Continuation", v => this.Continuation = (PlanContinuationPolicy)v);

            if (tag.TryLoadDefOut<DesignationDef>("Designation", out var designation))
                this.Designation = designation;
            if (tag.TryLoadInt("ZoneID", out var zoneID))
                this._zoneID = zoneID;
        }
        internal void SyncToClients(IDataWriter w)
        {
            w.Write(this.Def);
            this.TargetA.Write(w);
            var hasOrder = this.Order is not null;
            w.Write(hasOrder);
            if (hasOrder)
                w.Write(this.Order.Id);
        }
        internal void SyncFromServer(NetEndpoint provider, IDataReader r)
        {
            this.Def = r.ReadDef<PlanDef>();
            this.TargetA = TargetArgs.Read(provider, r);
            if (r.ReadBoolean())
            {
                var orderid = r.ReadInt32();
                this.Order = this.TargetA.Map.Town.CraftingManager.GetOrder(orderid);
            }
        }
        public void ObjectLoaded(GameObject parent)
        {

        }
        internal void MapLoaded(GameObject parent)
        {
            this.TargetA.ResolveReferences(parent.Map);
            this.TargetB.ResolveReferences(parent.Map);
            this.TargetC.ResolveReferences(parent.Map);
            foreach (var tar in this.TargetsA)
                tar.ResolveReferences(parent.Map);
            foreach (var tar in this.TargetsB)
                tar.ResolveReferences(parent.Map);
            foreach (var tar in this.TargetsC)
                tar.ResolveReferences(parent.Map);
            foreach (var q in this.TargetQueues)
                foreach (var t in q)
                    t.ResolveReferences(parent.Map);
            foreach (var t in this.GetCustomTargets())
                t.ResolveReferences(parent.Map);
            foreach (var t in this.PlacedObjects)
                t.ResolveReferences(parent.Map.World);
        }
        internal void AddTarget(TargetIndex index, Entity target, int count = -1)
        {
            this.AddTarget(index, new TargetArgs(target), count);
        }
        internal void AddTarget(TargetIndex index, TargetArgs target, int count = -1)
        {
            List<TargetArgs> t;
            List<int> a;
            switch (index)
            {
                case TargetIndex.A:
                    t = this.TargetsA;
                    a = this.AmountsA;
                    break;

                case TargetIndex.B:
                    t = this.TargetsB;
                    a = this.AmountsB;
                    break;

                case TargetIndex.C:
                    t = this.TargetsC;
                    a = this.AmountsC;
                    break;

                default:
                    throw new Exception();
            }
            t.Add(target);
            a.Add(count);
        }
        
        IEnumerable<TargetArgs> GetCustomTargets() { yield break; }
        public bool IsStillValid()
        {
            var map = this.Actor.Map;
            if (this.DesignationRequired && !map.Town.DesignationManager.IsDesignation(this.TargetA, this.Designation))
                return false;
            if (this.ZoneRequired && !this.Zone!.Contains(TargetA.Global))
                return false;
            return true;
        }
        internal bool ReserveAll()
        {
            return
                this.ReserveAll(TargetIndex.A) &&
                this.ReserveAll(TargetIndex.B) &&
                this.ReserveAll(TargetIndex.C);
        }
        internal bool ReserveAll(TargetIndex sourceIndex)
        {
            /// TODO: interperet amount by target type:
            /// for entities do if -1 then amount = entity.stacksize
            /// for intvec3 and blockentities, do amount  = 1
            if (this.GetTarget(sourceIndex) is TargetArgs singleTarget && singleTarget != TargetArgs.Null)
            {
                var amountSpecified = this.GetAmount(sourceIndex);
                var amountToReserve = singleTarget.Type switch
                {
                    TargetType.Entity => amountSpecified > 0 ? amountSpecified : singleTarget.Object.StackSize,
                    _ => 1
                };
                this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this, singleTarget, amountToReserve);
            }
            var targets = this.GetTargetQueue(sourceIndex);
            var amounts = this.GetAmountQueue(sourceIndex);
            var count = targets.Count;
            if (count != amounts.Count)
                throw new Exception();
            for (int i = 0; i < count; i++)
            {
                var target = targets[i];
                var amount = amounts[i];
                if (!this.Actor.Map.Town.ReservationManager.Reserve(this.Actor, this, target, amount))
                    return false;
            }
            return true;
        }
    }
}
