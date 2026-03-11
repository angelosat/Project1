using Project1.Core.Animations;
using Project1.Core.Assets;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Inventory;
using Project1.Core.Networking;
using Project1.Core.Systems.Materials;
using Project1.Core.UI.Hud;
using Project1.Framework;
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
                    SendCancel(net, player, [.. items]);
            }
        }
        static readonly IconButton IconCancel = new IconButton(new Icon(ItemContent.HammerFull), Icon.Cross)
        { HoverText = "Cancel crafting" }
            .AddLabel("Cancel");
        public ContainerList Contents = [];
        Dictionary<BoneDef, MaterialDef> _materialBindings;
        public Reaction.Product.ProductMaterialPair Product;
        private bool _initialized;

        public int OrderId { get; private set; }
        public override EntityCompDef CompDef => EntityCompDefOf.UnfinishedItem;
        public override string Name => "UnfinishedItem";
        public IReadOnlyDictionary<BoneDef, MaterialDef> MaterialBindings => this._materialBindings;

        internal EntityCreationRequest GetCreationRequest()
        {
            return new EntityCreationRequest(this.Owner.Profile, null);
        }
        internal void Initialize(Actor author, CraftingOrder order, IEnumerable<MaterialDef> bindings)
        {
            if (this._initialized)
                throw new InvalidOperationException();
            this._initialized = true;
            this._materialBindings = CraftingSystem.MapBonesToMaterials(this.Owner.Profile, bindings);
            this.OrderId = order.Id;
        }
        internal override IEnumerable<Control> GetSelectionInfo()
        {
            //var box = new GroupBox();
            foreach (var b in this._materialBindings)
                //box.AddControlsBottomLeft(new GroupBox().AddControlsLineWrap(
                yield return new GroupBox().AddControlsLineWrap(
                    new LabelNew($"{b.Key.LabelReadable}"), 
                    new LabelNew($"[{b.Value.LabelReadable}]") { TextColor = b.Value.Color });
            //info.AddInfo(box);
        }
        internal override void GetQuickButtons(SelectionManager info, GameObject parent)
        {
            return;
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
            }
            this.Owner.SyncDispose();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            this.Product.Save(tag, "Product");
            this.OrderId.Save(tag, "Order");
            this.Contents.Save(tag, "Contents");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.Product = new(tag["Product"]);
            this.OrderId = (int)tag["Order"].Value;
            this.Contents.Load(tag["Contents"]);
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.OrderId);
            w.Write([..this.MaterialBindings.Values]);
        }

        public override void Read(IDataReader r)
        {
            this.OrderId = r.ReadInt32();
            this._materialBindings = CraftingSystem.MapBonesToMaterials(this.Owner.Profile, r.ReadListDef<MaterialDef>());
        }

        public new class Spec : Spec<UnfinishedItemComp> { }
    }
}
