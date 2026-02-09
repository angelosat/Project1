using System.Collections.Generic;
using System.Linq;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework.Helpers;
using Project1.Framework;
using Project1.Core.Assets;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Net;
using Project1.Core.Inventory;
using Project1.Core.Entities;
using Project1.Core.UI.Hud;

namespace Project1.Core.Legacy.Crafting
{
    class UnfinishedItemComp : EntityComp
    {
        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pCancel;
            static Packets()
            {
                pCancel = Registry.PacketHandlers.Register(ReceiveCancel);
            }

            public static void SendCancel(NetEndpoint net, PlayerData player, List<TargetArgs> obj)
            {
                var w = net.BeginPacket(pCancel);

                w.Write(player.ID);
                w.Write(obj.Select(t => t.Object.RefId).ToList());
            }
            private static void ReceiveCancel(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var refIDs = r.ReadListInt32();
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
        public Actor Creator => this._creatorCached ??= this.Owner.World.GetEntity<Actor>(this._creator);
        CraftOrder _orderCached;
        public CraftOrder Order => this._orderCached ??= this.Owner.Map.Town.CraftingManager.GetOrder(this._orderid);
        public ContainerList Contents = new();
        internal void SetProduct(Reaction.Product.ProductMaterialPair product, Actor creator, CraftOrder order)
        {
            this._orderCached = order;
            this._creatorCached = creator;
            this.Product = product;
            this.Progress.Max = product.WorkAmount;
            this.Owner.Physics.SetWeight(product.Product.Physics.Weight);
            this.Owner.Name = $"Unfinished {product.Product.Def.LabelReadable}";
            this.Owner.SetMaterial(product.Product.PrimaryMaterial);
            foreach (var item in product.RequirementsNew.Values.Select(t => t.Object as Entity).Distinct())
                this.Contents.Add(item);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsVertically(
                this.Progress.GetGui(),
                new Label($"Creator: {this.Creator.Name}"),
                Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }
        internal override void GetSelectionInfo(SelectionManager info, GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsVertically(
                this.Progress.GetGui(),
                new Label($"Creator: {this.Creator.Name}"),
                Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }
        internal override void GetQuickButtons(SelectionManager info, GameObject parent)
        {
            info.AddButton(IconCancel, items => Packets.SendCancel(parent.Net, parent.Net.GetPlayer(), items), parent);
        }
        private void Cancel()
        {
            if (this.Owner.Net is not Server)
                return;
            foreach(var item in this.Contents.ToList()) //tolist because spawning them automatically removes them from their container
            {
                item.Global = this.Owner.Global;
                item.SyncSpawnNew(this.Owner.Map);
            }
            this.Owner.SyncDispose();
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
        public override void Write(IDataWriter w)
        {
            this.Product.Write(w);
            this.Progress.Write(w);
            w.Write(this.Creator.RefId);
            w.Write(this.Order.ID);
            this.Contents.Write(w);
        }

        public override void Read(IDataReader r)
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
        public new class Props : Spec<UnfinishedItemComp> { }
    }
}
