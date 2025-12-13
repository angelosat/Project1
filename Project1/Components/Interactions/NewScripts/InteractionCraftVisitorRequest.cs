using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Core;

namespace Start_a_Town_
{
    class InteractionCraftVisitorRequest : InteractionPerpetual
    {
        int OrderID;
        int ShopID;
        Progress Progress = new();
        readonly List<ObjectRefIDsAmount> PlacedObjects = new();
        Dictionary<string, ObjectRefIDsAmount> IngredientsUsed = new();
        public InteractionCraftVisitorRequest(int orderID, List<ObjectAmount> placedObjects)
              : this()
        {
            this.OrderID = orderID;
            this.PlacedObjects = placedObjects.Select(o=>new ObjectRefIDsAmount(o.Object, o.Amount)).ToList();
        }
        public InteractionCraftVisitorRequest(int shopid, int orderID, List<ObjectRefIDsAmount> placedObjects)
            : this()
        {
            this.ShopID = shopid;
            this.OrderID = orderID;
            this.PlacedObjects = placedObjects;
        }
        public InteractionCraftVisitorRequest()
            : base("Produce")
        {

        }

        public InteractionCraftVisitorRequest(Workplace shop, CraftOrder order, Dictionary<string, ObjectRefIDsAmount> ingredientsUsed) : this()
        {
            this.ShopID = shop.ID;
            this.OrderID = order.ID;
            this.IngredientsUsed = ingredientsUsed;
        }

      
        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            if (this.Progress.Value < this.Progress.Max)
                return;

            CraftOrder order = a.Town.GetShop(this.ShopID).GetOrder(this.OrderID);

            var product = ProduceWithMaterialsOnTopNew(a, t.Global, order);

            if (a.Net is Net.Server)
            {
                var task = a.CurrentTask;
                if (task != null)
                    task.Product = new TargetArgs(product);
            }
            this.Finish();
        }
        internal override void OnToolContact()
        {
            this.Progress.Value += 25;
        }
       
        public override object Clone()
        {
            return new InteractionCraftVisitorRequest(this.ShopID, this.OrderID, this.PlacedObjects);
        }

        GameObject ProduceWithMaterialsOnTopNew(Actor actor, Vector3 global, CraftOrder order)
        {
            var ingr = this.IngredientsUsed.ToDictionary(vk => vk.Key, vk => new ObjectAmount(actor.World.GetEntity(vk.Value.Object), vk.Value.Amount));
            var reaction = order.Reaction;
            var product = reaction.Products.First().Make(actor as Actor, reaction, ingr);
            product.ConsumeMaterials();
            actor.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompRefuelable>()?.ConsumePower(actor.Map, order.Reaction.Fuel);

            order.Complete(actor);

            if (actor.Net is not Server server)
                return null;
            actor.CurrentTask?.AddCraftedItem(product.Product as Entity);
            product.Product.Global = global + Vector3.UnitZ;
            product.Product.SyncInstantiate(server);
            //actor.Map.SyncSpawn(product.Product, global.Above(), Vector3.Zero);
            actor.Map.SpawnAndSync(product.Product as Entity, global.Above(), Vector3.Zero);

            return product.Product;
        }
        


        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.ShopID.Save("ShopID"));
            tag.Add(this.OrderID.Save("OrderID"));
            tag.Add(this.Progress.Save("CraftProgress"));
            tag.Add(this.PlacedObjects.SaveNewBEST("PlacedItems"));
        }
        public override void LoadData(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault("ShopID", out this.ShopID);
            tag.TryGetTagValueOrDefault("OrderID", out this.OrderID);
            this.Progress = new Progress(tag["CraftProgress"]);
            this.PlacedObjects.TryLoadMutable(tag, "PlacedItems");
        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.ShopID);
            w.Write(this.OrderID);
            this.Progress.Write(w);
            this.PlacedObjects.Write(w);
            this.IngredientsUsed.WriteNew(w, k => w.Write(k), v => v.Write(w));
        }
        protected override void ReadExtra(IDataReader r)
        {
     
            this.ShopID = r.ReadInt32();
            this.OrderID = r.ReadInt32();
            this.Progress = new Progress(r);
            this.PlacedObjects.ReadMutableNew(r);
            this.IngredientsUsed.ReadFromFlat(r, r => r.ReadString(), r => new ObjectRefIDsAmount().Read(r) as ObjectRefIDsAmount);
        }

        public override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}
