using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Crafting
{
    class InteractionCraftingLogic : InteractionLogic
    {
        public sealed class Context : InteractionContext
        {
            BlockWorkstationComp _comp;
            public BlockWorkstationComp Comp => this._comp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockWorkstationComp>();
            public override float ProgressPercentage => this.Actor.Work.Task.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();
        public override bool CanFinish(InteractionContext ctx) => this.CanFinish((Context)ctx);
        bool CanFinish(Context ctx) => this.CanPerform(ctx);
        public override bool CanPerform(InteractionContext ctx) => this.CanPerform((Context)ctx);
        bool CanPerform(Context ctx)
        {
            return ctx.Comp.IngredientsInPlace(ctx.Actor.CurrentTask.TargetsA);
        }
    }
    class InteractionCraftingNew : InteractionToolUse
    {
        Progress _progress = new();
        protected override Progress ProgressNew => this._progress;

        protected override float WorkDifficulty => 1;//throw new System.NotImplementedException();


        public InteractionCraftingNew() : base("Craft") 
        {
            this.SkillAwardType = SkillAwardTypes.OnFinish;
        }
        public override float Percentage => this.TotalWorkAmount / 100; // HACK
        internal override void AddProgress(int v)
        {
            this.TotalWorkAmount += v;
        }
        protected override void Done()
        {
            var actor = this.Actor;
            if (actor.Net.IsClient)
                return;
            var map = actor.Map;
            var plan = actor.CurrentTask;
            var order = plan.Order;
            var workstation = this.Target;
            var inSlots = plan.TargetsA.Select(t => t.Entity as Entity);
            var creationReq = order.GetCreationRequest(); /*new EntityCreationRequest(order.Refinement, null, stackSize: 1);*/
            var targetBones = order.GetSlotMapping();
            var mapping = targetBones.Zip(inSlots);
            foreach (var pair in mapping)
            {
                creationReq.Override(pair.First, pair.Second.Body.Material);
                map.World.DisposeEntity(pair.Second);
            }
            var product = EntityFactory.Create(creationReq);
            map.World.Register(product);
            map.Spawn(product, workstation.Global.Above(), Vector3.Zero);
        }
        protected override void OnApplyWork(int workAmount)
        {
            this._progress.Value += workAmount;// 25;
            this.Actor.Map.Events.Post(new InteractionProgressEvent(this.Actor, workAmount));
        }
        
        internal override void ResolveReferences()
        {
            //this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstationOld>().GetOrder(this.OrderID);
            //this._progress = this.UnfinishedItem.GetComponent<UnfinishedItemComp>().Progress;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return null;
        }

        protected override Color GetParticleColor()
        {
            return default;
        }

        protected override ToolUseDef GetToolUse() => null;

        protected override SkillDef GetSkill() => SkillDefOf.Crafting;
    }

    

    class InteractionCrafting : InteractionToolUse
    {
        CraftOrder Order;
        Reaction.Product.ProductMaterialPair Product;
        readonly List<ObjectRefIDsAmount> PlacedObjects = new();
        int _unfinishedRefID;
        Entity _unfinished;
        Entity UnfinishedItem => this._unfinished ??= this.Actor.World.GetEntity(this._unfinishedRefID);
        Progress _progress = new();

        //protected override float Progress => this._progress.Percentage;
        protected override float WorkDifficulty => 1;
        //protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnFinish;

        int _orderID;
        private int OrderID => this.Order?.ID ?? this._orderID;

        public InteractionCrafting(CraftOrder order, List<ObjectAmount> placedObjects)
            : this()
        {
            this.Order = order;
            this.PlacedObjects = placedObjects.Select(o => new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
        }
        public InteractionCrafting(CraftOrder order, List<ObjectAmount> placedObjects, Entity unfinishedItem)
           : this()
        {
            this.Order = order;
            this.PlacedObjects = placedObjects.Select(o => new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
            this._unfinished = unfinishedItem;
        }
        public InteractionCrafting(CraftOrder order, List<ObjectRefIDsAmount> placedObjects)
            : this()
        {
            this.Order = order;
            this.PlacedObjects = placedObjects;
        }
        public InteractionCrafting()
            : base("Produce")
        {
            this.DrawProgressBar(() => this.Target.Global.Above(), () => this.Progress, () => this.Order.Label);
        }

        //public override object Clone()
        //{
        //    return new InteractionCrafting(this.Order, this.PlacedObjects);
        //}
        protected override void Init()
        {
            if (this.Order.Reaction.CreatesUnfinishedItem)
            {
                this._progress = this.UnfinishedItem.GetComponent<UnfinishedItemComp>().Progress;
                this.Order.UnfinishedItem = this.UnfinishedItem;
                return;
            }
            var actor = this.Actor;
            var ingr = this.PlacedObjects.Select(o => new ObjectAmount(actor.World.GetEntity(o.Object), o.Amount)).ToList();
            this.Product = this.Order.Reaction.Products.First().Make(actor, this.Order.Reaction, ingr);
            this._progress.Max = this.Product.WorkAmount;
        }
        GameObject ProduceWithMaterialsOnTopNew()
        {
            var global = this.Target.Global;
            var actor = this.Actor;
            var product = this.Product ?? this.UnfinishedItem.GetComponent<UnfinishedItemComp>().Product;
            var reaction = this.Order.Reaction;
            var order = this.Order;
            var skillAwardAmount = 100;
            actor[reaction.CraftSkill].Award(skillAwardAmount);

            if (!order.Reaction.CreatesUnfinishedItem)
                product.ConsumeMaterials();
            else if (actor.Net is Server)
                this.UnfinishedItem.SyncDispose();

            if (order.Reaction.Fuel > 0)
                actor.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompRefuelable>()?.ConsumePower(actor.Map, order.Reaction.Fuel);

            order.Complete(actor);

            if (actor.Net is not Server server)
                return null;
            if (!product.Product.Exists) // HACK for when the product is one of the ingredients (already existing on top of the workstation)
            {
                product.Product.Global = global + Vector3.UnitZ;
                product.Product.SyncInstantiate(server);
                //actor.Map.SyncSpawn(product.Product, global.Above(), Vector3.Zero);
                actor.Map.SpawnAndSync(product.Product as Entity, global.Above(), Vector3.Zero);
            }
            return product.Product;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.TrySaveRef(this.Order, "Order");
            tag.Add(this._progress.Save("CraftProgress"));
            tag.Add(this.PlacedObjects.SaveNewBEST("PlacedItems"));
            if (this.Product is not null)
                this.Product.Save(tag, "Product");
            tag.Add("Unfinished", this.UnfinishedItem?.RefId ?? -1);
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryLoadRef("Order", out this.Order);
            this._progress = new Progress(tag["CraftProgress"]);
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
            tag.TryGetTag("Product", t => this.Product = new Reaction.Product.ProductMaterialPair(t));
            tag.TryGetTagValueOrDefault("Unfinished", out this._unfinishedRefID);
        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.Order.ID);
            this._progress.Write(w);
            this.PlacedObjects.Write(w);
            w.Write(this.Product is not null);
            if (this.Product is not null)
                this.Product.Write(w);
            w.Write(this.UnfinishedItem?.RefId ?? -1);
        }
        protected override void ReadExtra(IDataReader r)
        {
            var orderID = r.ReadInt32();
            if (this.Actor.Map is not null)
                this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstationOld>().GetOrder(orderID);
            else
                this._orderID = orderID;
            this._progress = new Progress(r);
            this.PlacedObjects.ReadMutableNew(r);
            if (r.ReadBoolean())
                this.Product = new Reaction.Product.ProductMaterialPair(r);
            this._unfinishedRefID = r.ReadInt32();
        }

        internal override void ResolveReferences()
        {
            this.Order = this.Actor.Map.GetBlockEntity(this.Target.Global).GetComp<BlockEntityCompWorkstationOld>().GetOrder(this.OrderID);
            this._progress = this.UnfinishedItem.GetComponent<UnfinishedItemComp>().Progress;
        }

        protected override void OnApplyWork(int workAmount)
        {
            this._progress.Value += workAmount;// 25;
        }

        protected override void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            var product = this.ProduceWithMaterialsOnTopNew();
            if (a.Net is Server)
            {
                var task = a.CurrentTask;
                if (task is not null)
                    task.Product = new TargetArgs(product);
            }
        }

        protected override ToolUseDef GetToolUse()
        {
            return this.Order.Reaction.Labor?.ToolUse;
        }

        protected override SkillDef GetSkill()
        {
            return this.Order.Reaction.CraftSkill;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return null;
        }

        protected override Color GetParticleColor()
        {
            return default;
        }
    }
}
