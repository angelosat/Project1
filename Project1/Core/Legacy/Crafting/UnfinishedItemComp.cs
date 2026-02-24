using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Inventory;
using Project1.Core.Materials;
using Project1.Core.Networking;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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

            public static void SendCancel(NetEndpoint net, PlayerData player, List<Entity> selection)
            {
                var w = net.BeginPacket(pCancel);
                w.Write(player.ID);
                w.Write(selection.Select(t => t.RefId).ToList());
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
                    //SendCancel(net, player, items.Select(o => new TargetArgs(o)).ToList());
                    SendCancel(net, player, [.. items]);
            }
        }
        static readonly IconButton IconCancel = new IconButton(new Icon(ItemContent.HammerFull), Icon.Cross) 
            { HoverText = "Cancel crafting" }
            .AddLabel("Cancel");
        public override EntityCompDef CompDef => EntityCompDefOf.UnfinishedItem;
        public override string Name => "UnfinishedItem";
        public Reaction.Product.ProductMaterialPair Product;
        public Progress Progress = new();
        ProgressInt ProgressInt = new(100);
        public float ProgressPercentage => this.ProgressInt.Percentage;
        int _orderid;
        EntityRefId _authorId;
        public Actor Author
        {
            get => field ??= this.Owner.World.GetEntity<Actor>(this._authorId);
            private set
            {
                field = value;
                this._authorId = value.RefId;
            }
        }
        CraftingOrder _orderCached;
        public CraftingOrder Order => this._orderCached ??= this.Owner.Map.Town.CraftingManagerNew.GetOrder(this._orderid);
        public ContainerList Contents = [];
        readonly Dictionary<BoneDef, MaterialDef> _materialBindings = [];
        public IReadOnlyDictionary<BoneDef, MaterialDef> MaterialBindings => this._materialBindings;
        private bool _initialized;
        internal void ApplyWork(int workAmount)
        {
            this.ProgressInt.ApplyDelta(workAmount);
        }
        internal void Initialize(Actor author, IEnumerable<(BoneDef bone, MaterialDef material)> bindings)
        {
            if (this._initialized)
                throw new InvalidOperationException();
            this._initialized = true;
            this._materialBindings.Clear();
            foreach (var p in bindings)
                this._materialBindings.Add(p.bone, p.material);
            this.Author = author;
        }
        internal void SetProduct(Reaction.Product.ProductMaterialPair product, Actor creator, CraftingOrder order)
        {
            this._orderCached = order;
            this.Author = creator;
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
                new Label($"Creator: {this.Author.Name}"),
                Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }
        internal override void GetSelectionInfo(SelectionManager info, GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsVertically(
                this.Progress.GetGui(),
                new Label($"Creator: {this.Author.Name}"),
                Label.ParseNewNew("Order: ", this.Order).ToGroupBoxHorizontally()
                );
            info.AddInfo(box);
        }
        internal override void GetQuickButtons(SelectionManager info, GameObject parent)
        {
            info.AddButton(IconCancel, items => Packets.SendCancel(parent.Net, parent.Net.GetPlayer(), [items as Entity]), parent);
        }
        private void Cancel()
        {
            if (this.Owner.Net is not Server)
                return;
            foreach(var item in this.Contents.ToList()) //tolist because spawning them automatically removes them from their container
            {
                item.Global = this.Owner.Global;
                throw new NotImplementedException();
                //item.SyncSpawnNew(this.Owner.Map);
            }
            this.Owner.SyncDispose();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            this.Product.Save(tag, "Product");
            this.Author.RefId.Save(tag, "Creator");
            this.Order.Id.Save(tag, "Order");
            this.Progress.Save(tag, "Progress");
            this.Contents.Save(tag, "Contents");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.Product = new(tag["Product"]);
            this._authorId = (int)tag["Creator"].Value;
            this._orderid = (int)tag["Order"].Value;
            this.Progress.Load(tag["Progress"]);
            this.Contents.Load(tag["Contents"]);
        }
        public override void Write(IDataWriter w)
        {
            this.Product.Write(w);
            this.Progress.Write(w);
            w.Write(this.Author.RefId);
            w.Write(this.Order.Id);
            this.Contents.Write(w);
        }

        public override void Read(IDataReader r)
        {
            this.Product = new(r);
            this.Progress.Read(r);
            this._authorId = r.ReadInt32();
            this._orderid = r.ReadInt32();
            this.Contents.Read(r);
        }
        public override void OnDispose()
        {
            this.Order.UnfinishedItem = null;
        }

        

        public new class Spec : Spec<UnfinishedItemComp> { }
    }
}
