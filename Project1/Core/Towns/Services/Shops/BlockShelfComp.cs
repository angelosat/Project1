using Project1.Core;
using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Systems.Consumables;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Services.Shops;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

#nullable enable

sealed class BlockShelfComp_Gui : SelectionBoundControl
{
    readonly ComboBoxFinal<QualityDef> ComboQuality;
    //ComboBoxFinal<Def> ComboProfile;
    readonly Table<Def> TableSale;
    static readonly List<Def> ValidProfiles = [];// [Def.Get<ToolProfileDef>(), Def.Get<ConsumableDef>()];
    static IEnumerable<QualityDef> AllQualities => field ??= Def.Get<QualityDef>();
    BlockShelfComp? Comp;
    static BlockShelfComp_Gui()
    {
        foreach (var def in Def.Get<ToolProfileDef>())
            ValidProfiles.Add(def);
        foreach (var def in Def.Get<ConsumableDef>())
            ValidProfiles.Add(def);
        foreach (var def in Def.Get<MaterialRefinementDef>())
            ValidProfiles.Add(def);
    }
    public BlockShelfComp_Gui()
    {
        this.TableSale = new Table<Def>()
            .AddColumn("def", 92, d => new LabelNew(d), 1)
            .AddColumn("count", 32, d => new LabelNew(() => $"{this.Comp?.Map.Town.Shops.CountForSale(d) ?? 0}"))
            .AddColumn("ticked", CheckBoxFinalNew.DefaultBounds.Width, d => new CheckBoxFinalNew(() => { }, () => false));
        this.TableSale.AddItems(ValidProfiles);

        this.ComboQuality = new(
            AllQualities, 
            100, 
            q => q.LabelReadable, 
            q => Ingame.Instance.Events.Post(new PlayerToggleShelfQualityFilterEvent(this.Comp, q)), 
            () => this.Comp!.MinQuality);

    }

    protected internal override void OnBind(ISelectable selectable)
    {
        if (selectable is not BlockEntity be || !be.TryGetComp<BlockShelfComp>(out var comp))
            return;
        this.Controls.Clear();

        this.Comp = comp;
      
        this.AddControlsVertically(
            ScrollableBoxNewNewNew.FromWidth(this.TableSale, this.TableSale.RowWidth, 400).ToPanelLabeled("Toggle sales"),
            this.ComboQuality);

        this.Comp.Map.Town.Shops.ItemsForSaleToggled += Shops_ItemsForSaleToggled;
        this.InvalidateOn(this.Comp.Notifier);
        this.TableSale.Invalidate(true);
    }

    private void Shops_ItemsForSaleToggled((IEnumerable<Entity> added, IEnumerable<Entity> removed) obj)
    {
        this.TableSale.Invalidate(true);
    }
    protected override void OnDetached()
    {
        this.Comp!.Map.Town.Shops.ItemsForSaleToggled -= Shops_ItemsForSaleToggled;
    }
}

namespace Project1.Core.Towns.Services.Shops
{
    internal sealed class BlockShelfComp : BlockComp
    {
        internal new sealed class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockShelfComp);

            public override BlockShelfComp CreateComp() => new();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Shelf;

        ZoneId InputStockpile = ZoneId.Null;

        //internal Entity GetDisplayedItem() => this.Parent.Map.GetEntitiesAt(this.Parent.OriginGlobal.Above).FirstOrDefault();
        internal void SetInput(ZoneId stockpileId)
        {
            this.InputStockpile = stockpileId;
        }
        internal Def? Filter;
        internal QualityDef MinQuality = QualityDefOf.Common;
        internal bool Accepts(Entity item) => item.Profile == this.Filter && item.Quality == this.MinQuality;

        internal override IEnumerable<(string label, Type type)> GetSelectionTabs()
        {
            yield return ("Display", typeof(BlockShelfComp_Gui));
        }

        internal void SetQuality(QualityDef q)
        {
            this.MinQuality = q;
            //this.Map.Events.Post(new BlockEntityCompUpdatedEvent(this));
            this.Notifier.Notify();
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.Filter);
            w.Write(this.MinQuality);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Filter = r.ReadDef();
            this.MinQuality = r.ReadDef<QualityDef>();
            return this;
        }
    }

    internal record struct PlayerToggleShelfQualityFilterEvent(BlockShelfComp Comp, QualityDef Quality) : IEventPayload;

    [EnsureStaticCtorCall]
    internal static class Packets_Shelf
    {
        static readonly PacketId _pPlayerQualityToggle = Registry.PacketHandlers.Register(ReceivePlayerQualityToggle);

        static Packets_Shelf()
        {
            Registry.PlayerInputEventHooks.Register<PlayerToggleShelfQualityFilterEvent>(HandlePlayerToggleShelfQualityFilter);
        }

        private static void HandlePlayerToggleShelfQualityFilter(PlayerToggleShelfQualityFilterEvent e)
        {
            var net = e.Comp.World.Net;
            if (net.IsServer)
                e.Comp.SetQuality(e.Quality);
            Send(e, net);
        }

        private static void Send(PlayerToggleShelfQualityFilterEvent e, NetEndpoint net)
        {
            net.BeginPacketImmediate(_pPlayerQualityToggle)
                            .Write(e.Comp.Map.ID)
                            .Write(e.Comp.Parent.OriginGlobal)
                            .Write(e.Quality);
        }

        private static void ReceivePlayerQualityToggle(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var map = endpoint.World.Get(r.ReadId<MapId>());
            var comp = map.GetBlockEntityComp<BlockShelfComp>(r.ReadIntVec3());
            var q = r.ReadDef<QualityDef>();
            comp.SetQuality(q);
            if (endpoint.IsServer)
                Send(new(comp, q), endpoint);
        }
    }
}