using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public enum TargetIndex { A, B, C, Tool = 15 }
    public class AITask
    {
        public TargetArgs GetTarget(TargetIndex targetInd)
        {
            return targetInd switch
            {
                TargetIndex.Tool => this.Tool,
                TargetIndex.A => this.TargetA,
                TargetIndex.B => this.TargetB,
                TargetIndex.C => this.TargetC,
                _ => throw new Exception(),
            };
        }

        internal TargetArgs GetTarget(int targetInd)
        {
            return this.GetTarget((TargetIndex)targetInd);
        }

        internal bool ReserveAll(Actor actor, TargetIndex sourceIndex)
        {
            var targets = this.GetTargetQueue(sourceIndex);
            var amounts = this.GetAmountQueue(sourceIndex);
            var count = targets.Count;
            if (count != amounts.Count)
                throw new Exception();
            for (int i = 0; i < count; i++)
            {
                var target = targets[i];
                var amount = amounts[i];
                if (!actor.Town.ReservationManager.Reserve(actor, target, amount))
                    return false;
            }
            return true;
        }
        internal bool Reserve(Actor actor, TargetIndex index)
        {
            return actor.Town.ReservationManager.Reserve(actor, this.GetTarget(index), this.GetAmount(index));
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

        internal AITask SetTarget(TargetIndex targetInd, GameObject target, int amount)
        {
            this.SetAmount(targetInd, amount);
            return this.SetTarget(targetInd, new TargetArgs(target));
        }

        //internal AITask SetTarget(TargetIndex targetInd, MapBase map, Vector3 target)
        //{
        //    return this.SetTarget(targetInd, target.At(map));
        //}
       
        internal AITask SetTarget(TargetIndex targetInd, TargetArgs targetArgs)
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

        internal void AddPlacedObject(GameObject hauledObj)
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

        public static AITask Load(SaveTag tag)
        {
            var task = new AITask();
            task.LoadData(tag);
            return task;
        }

        internal static void Initialize()
        {
        }
        public AITask()
        {
            this.ID = ReservationManager.GetNextTaskID();
        }
        public virtual string Name => "unnamed task";
        public TargetArgs Tool = TargetArgs.Null;
        public TargetArgs TargetA = TargetArgs.Null;
        public TargetArgs TargetB = TargetArgs.Null;
        public TargetArgs TargetC = TargetArgs.Null;
        public List<TargetArgs> TargetsA = new();
        public List<TargetArgs> TargetsB = new();
        public List<TargetArgs> TargetsC = new();
        public int Count;
        public List<List<TargetArgs>> TargetQueues = new();
        public List<List<int>> AmountQueues = new();
        public int AmountA = -1, AmountB = -1, AmountC = -1;
        public List<int> AmountsA = new();
        public List<int> AmountsB = new();
        public List<int> AmountsC = new();
        public List<ObjectAmount> PlacedObjects = new();
        public List<Entity> CraftedItems = new();

        public List<Func<bool>> FailConditions = new();
        public CraftOrder Order;
        public Dictionary<string, ObjectRefIDsAmount> IngredientsUsed = new();

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
        public bool IsCancelled => this.Cancelled;

        public bool IsReserved => this.ReservedBy > -1;

        public TaskDef Def;
        public int ID { get; internal set; }

        public AITask(TaskDef taskDef)
        {
            this.Def = taskDef;
        }

        public AITask(Type behaviorType) : this()
        {
            this.BehaviorType = behaviorType;
        }
        public AITask(Type behaviorType, TargetArgs targetA) : this()
        {
            this.BehaviorType = behaviorType;
            this.SetTarget(TargetIndex.A, targetA);
        }
        public AITask(TaskDef def, TargetArgs targetA) : this()
        {
            this.Def = def;
            this.SetTarget(TargetIndex.A, targetA);
        }
        public AITask(TaskDef def, TargetArgs targetA, TargetArgs targetB) : this()
        {
            this.Def = def;
            this.SetTarget(TargetIndex.A, targetA);
            this.SetTarget(TargetIndex.B, targetB);
        }
        public AITask(Type behaviorType, TargetArgs targetA, TargetArgs targetB) : this()
        {
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

        Type _BehaviorType;

        public Type BehaviorType
        {
            get => this.Def?.BehaviorClass ?? this._BehaviorType;
            set => this._BehaviorType = value;
        }

        public BehaviorPerformTask CreateBehavior(Actor actor)
        {
            var behav = Activator.CreateInstance(this.BehaviorType) as BehaviorPerformTask;
            behav.Actor = actor;
            behav.Task = this;
            return behav;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            tag.Add(this.ID.Save("ID"));
            tag.Add(this.Tool.Save("Tool"));
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

            tag.Add(this.PlacedObjects.SaveOld("PlacedItems"));
            tag.Add(this.Count.Save("Count"));
            tag.TrySaveRef(this.Order, "Order");
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

            this.AddSaveData(tag);
            return tag;
        }
        protected virtual void AddSaveData(SaveTag tag)
        {

        }
        public virtual void LoadData(SaveTag tag)
        {
            this.Def = tag.LoadDef<TaskDef>("Def");
            tag.TryGetTagValue<int>("ID", t => this.ID = t);
            tag.TryGetTag("TargetA", t => this.TargetA = new TargetArgs(t));
            tag.TryGetTag("TargetB", t => this.TargetB = new TargetArgs(t));
            tag.TryGetTag("TargetC", t => this.TargetC = new TargetArgs(t));

            tag.TryGetTag("Tool", t => this.Tool = new TargetArgs(t));

            tag.TryGetTagValue("AmountA", out this.AmountA);
            tag.TryGetTagValue("AmountB", out this.AmountB);
            tag.TryGetTagValue("AmountC", out this.AmountC);

            tag.TryGetTag("TargetsA", t => this.TargetsA.Load(t));
            tag.TryGetTag("TargetsB", t => this.TargetsB.Load(t));
            tag.TryGetTag("TargetsC", t => this.TargetsC.Load(t));

            tag.TryGetTag("AmountsA", t => this.AmountsA.Load(t));
            tag.TryGetTag("AmountsB", t => this.AmountsB.Load(t));
            tag.TryGetTag("AmountsC", t => this.AmountsC.Load(t));

            tag.TryGetTagValue("TicksCounter", out this.TicksCounter);
            tag.TryGetTagValue("TicksWaited", out this.TicksWaited);
            tag.TryGetTagValue("TicksTimeout", out this.TicksTimeout);

            tag.TryGetTag("PlacedItems", t => this.PlacedObjects.Load(t));

            tag.TryLoadRef("Order", out this.Order);
            tag.TryGetTagValue("Count", out this.Count);
            tag.TryGetTag("Product", t => this.Product = new TargetArgs(t));
            tag.TryGetTagValue("Forced", out this.Forced);
            if (tag.TryGetTagValue("Queues", out List<SaveTag> queuestag))
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
            tag.TryGetTagValue("ShopID", out this.ShopID);
            tag.TryGetTagValue("QuestToAccept", out this.Quest);
            tag.TryGetTag("Transaction", v => this.Transaction = new Transaction(v));
        }

        internal virtual void Write(BinaryWriter w)
        {
            w.Write(this.GetType().FullName);
            w.Write(this.ID);
            this.Tool.Write(w);
            this.TargetA.Write(w);
            w.Write(this.AmountA);
            w.Write(this.TargetsA);
            w.Write(this.AmountsA);
            w.Write(this.TargetQueues.Count);
            foreach (var i in this.TargetQueues)
                w.Write(i);
            w.Write(this.AmountQueues.Count);
            foreach (var i in this.AmountQueues)
                w.Write(i);
            w.Write(this.Count);
            w.Write(this.Quest);
            w.Write(this.ShopID);

            w.Write(this.TicksWaited);
            w.Write(this.TicksCounter);
            w.Write(this.TicksTimeout);

            this.Transaction.Write(w);
        }
        protected virtual void Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Tool = TargetArgs.Read((INetEndpoint)null, r);
            this.TargetA = TargetArgs.Read((INetEndpoint)null, r);
            this.AmountA = r.ReadInt32();
            this.TargetsA = r.ReadListTargets();
            this.AmountsA = r.ReadListInt();
            var targetqueuecount = r.ReadInt32();
            for (int i = 0; i < targetqueuecount; i++)
                this.TargetQueues.Add(r.ReadListTargets());
            var amountqueuecount = r.ReadInt32();
            for (int i = 0; i < amountqueuecount; i++)
                this.AmountQueues.Add(r.ReadListInt());
            this.Count = r.ReadInt32();
            this.Quest = r.ReadInt32();
            this.ShopID = r.ReadInt32();

            this.TicksWaited = r.ReadInt32();
            this.TicksCounter = r.ReadInt32();
            this.TicksTimeout = r.ReadInt32();

            this.Transaction = new Transaction(r);
        }

        public void ObjectLoaded(GameObject parent)
        {

        }

        internal void MapLoaded(GameObject parent)
        {
            var net = parent.Net;
            this.Tool.InitializeProvider(net);
            this.TargetA.InitializeProvider(net);
            this.TargetB.InitializeProvider(net);
            this.TargetC.InitializeProvider(net);

            foreach (var tar in this.TargetsA)
                tar.InitializeProvider(net);
            foreach (var tar in this.TargetsB)
                tar.InitializeProvider(net);
            foreach (var tar in this.TargetsC)
                tar.InitializeProvider(net);

            foreach (var q in this.TargetQueues)
                foreach (var t in q)
                    t.InitializeProvider(net);
            foreach (var t in this.GetCustomTargets())
                t.InitializeProvider(net);
            foreach (var t in this.PlacedObjects)
                t.ResolveReferences(parent.Net);
        }
        internal void AddTargets(TargetIndex index, IEnumerable<(TargetArgs target, int amount)> targetsAmounts)
        {
            foreach (var (target, amount) in targetsAmounts)
                this.AddTarget(index, target, amount);
        }
        internal void AddTarget(TargetIndex index, GameObject target, int count = -1)
        {
            this.AddTarget(index, new TargetArgs(target), count);
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

        internal void AddTarget(TargetArgs target, int count = -1)
        {
            this.TargetsA.Add(target);
            this.AmountsA.Add(count);
        }

        protected virtual IEnumerable<TargetArgs> GetCustomTargets() { yield break; }
    }
}
