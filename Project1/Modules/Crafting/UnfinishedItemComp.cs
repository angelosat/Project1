using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    class UnfinishedItemComp : EntityComponent
    {
        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pCancel;
            static Packets()
            {
                pCancel = Network.RegisterPacketHandler(ReceiveCancel);
            }

            public static void SendCancel(INetEndpoint net, PlayerData player, List<TargetArgs> obj)
            {
                //var net = obj.First().Network;
                var w = net.GetOutgoingStreamOrderedReliable();
                w.Write(pCancel);
                w.Write(player.ID);
                w.Write(obj.Select(t => t.Object.RefId).ToList());
            }
            private static void ReceiveCancel(INetEndpoint net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var refIDs = r.ReadListInt();
                var items = net.World.GetEntities(refIDs);
                foreach (var i in items.ToList()) // tolist because cancelling changes the networkobjects collection
                    i.GetComponent<UnfinishedItemComp>().Cancel();
                if (net is Server)
                    SendCancel(net, player, items.Select(o => new TargetArgs(o)).ToList());
            }
        }

        static readonly IconButton IconCancel = new IconButton(new Icon(ItemContent.HammerFull), Icon.Cross) 
            { HoverText = "Cancel crafting" }
            .AddLabel("Cancel");

        public override string Name => "UnfinishedItem";

        public Reaction.Product.ProductMaterialPair Product;
        public Progress Progress = new();
        int _creator, _orderid;
        Actor _creatorCached;
        public Actor Creator => this._creatorCached ??= this.Parent.Net.GetNetworkObject<Actor>(this._creator);
        CraftOrder _orderCached;
        public CraftOrder Order => this._orderCached ??= this.Parent.Net.Map.Town.CraftingManager.GetOrder(this._orderid);
        public ContainerList Contents = new();
        public override object Clone()
        {
            return new UnfinishedItemComp();
        }

        internal void SetProduct(Reaction.Product.ProductMaterialPair product, Actor creator, CraftOrder order)
        {
            this._orderCached = order;
            this._creatorCached = creator;
            this.Product = product;
            this.Progress.Max = product.WorkAmount;
            this.Parent.Physics.SetWeight(product.Product.Physics.Weight);
            this.Parent.Name = $"Unfinished {product.Product.Def.Label}";
            ((Entity)this.Parent).SetMaterial(product.Product.PrimaryMaterial);
            foreach (var item in product.RequirementsNew.Values.Select(t => t.Object).Distinct())
                this.Contents.Add(item);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsVertically(
                this.Progress.GetGui(),
                new Label($"Creator: {this.Creator.Name}"),
                UI.Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }
        internal override void GetSelectionInfo(SelectionManager info, GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsVertically(
                this.Progress.GetGui(),
                new Label($"Creator: {this.Creator.Name}"),
                UI.Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }
        internal override void GetQuickButtons(SelectionManager info, GameObject parent)
        {
            info.AddButton(IconCancel, items => Packets.SendCancel(parent.Net, parent.Net.GetPlayer(), items), parent);
        }
        private void Cancel()
        {
            if (this.Parent.Net is not Server)
                return;
            foreach(var item in this.Contents.ToList()) //tolist because spawning them automatically removes them from their container
            {
                item.Global = this.Parent.Global;
                item.SyncSpawnNew(this.Parent.Map);
            }
            this.Parent.SyncDispose();
            //this.Order.UnfinishedItem = null;
        }
        internal override void SaveExtra(SaveTag tag)
        {
            this.Product.Save(tag, "Product");
            this.Creator.RefId.Save(tag, "Creator");
            this.Order.ID.Save(tag, "Order");
            this.Progress.Save(tag, "Progress");
            this.Contents.Save(tag, "Contents");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.Product = new(tag["Product"]);
            this._creator = (int)tag["Creator"].Value;
            this._orderid = (int)tag["Order"].Value;
            this.Progress.Load(tag["Progress"]);
            this.Contents.Load(tag["Contents"]);
        }
        public override void Write(BinaryWriter w)
        {
            this.Product.Write(w);
            this.Progress.Write(w);
            w.Write(this.Creator.RefId);
            w.Write(this.Order.ID);
            this.Contents.Write(w);
        }

        public override void Read(BinaryReader r)
        {
            this.Product = new(r);
            this.Progress.Read(r);
            this._creator = r.ReadInt32();
            this._orderid = r.ReadInt32();
            this.Contents.Read(r);
        }
        public override void OnDispose()
        {
            this.Order.UnfinishedItem = null;
        }
    }
}
