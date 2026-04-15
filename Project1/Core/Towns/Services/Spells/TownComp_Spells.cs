using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Services.Shops;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Spells;

sealed class Gui_TownSpellList : GroupBox
{
    readonly Table<(SpellDef spell, PriceTag_Spell tag)> Table;
    public Gui_TownSpellList()
    {
        var shops = Ingame.Net.MainViewport.Map.Town.Spells;

        this.Table = new Table<(SpellDef spell, PriceTag_Spell tag)>()
                    .AddColumn("item", 256, a => new LabelNew(a.spell))
                    .AddColumn("price", 48, a => new LabelNew(() => a.tag.Price.ToString()))
                    .AddColumn("tick", 32, a => new CheckBoxFinalNew(() => ToggleSpell(a.spell), () => a.tag.Enabled).InvalidateOn(a.tag.Notifier));
        this.Table.AddItems(shops.GetPriceList());

        var scrollbox = ScrollableBoxNewNewNew.FromWidth(this.Table, this.Table.RowWidth, Label.DefaultHeight * 16);
        this.Controls.Add(scrollbox.ToPanelLabeled("Price list"));
    }
    static void ToggleSpell(SpellDef spell)
        => Ingame.Instance.Events.Post(new PlayerTownSpellToggledEvent(Ingame.Net.MainViewport.Map, spell));
}

sealed class PriceTag_Spell(SpellDef spell, int price, bool enabled)
{
    internal ChangeNotifier Notifier = new();

    internal SpellDef Spell = spell;
    internal int Price = price;
    internal bool Enabled
    {
        get => field; private set
        {
            field = value;
            this.Notifier.Notify();
        }
    } = enabled;
    internal void Toggle()
        => this.Enabled = !this.Enabled;
}

public class TownComp_Spells : TownComp
{
    public override string Name => "Spells";

    readonly Dictionary<EntityRefId, ServiceRequest_Spell> _pendingRequestsByTarget = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Spell> _acceptedRequestsByCaster = [];
    //internal Dictionary<SpellDef, int> PriceList = new() { { SpellDefOf.Healing, 100 } };// 100 } };

    readonly Dictionary<SpellDef, PriceTag_Spell> _spellsOffered = [];
    //internal IEnumerable<PriceTag_Spell> GetPriceList() => this._spellsOffered.Values;
    internal IEnumerable<(SpellDef, PriceTag_Spell)> GetPriceList() => this._spellsOffered.Select(kv => (kv.Key, kv.Value));
    internal IEnumerable<PriceTag_Spell> GetAvailableSpells() => this._spellsOffered.Values.Where(value => value.Enabled);
    internal int GetPrice(SpellDef spell) => this._spellsOffered[spell].Price;
    internal void ToggleSpell(SpellDef spell)
        => this._spellsOffered[spell].Toggle();

    internal ICollection<ServiceRequest_Spell> PendingRequests => this._pendingRequestsByTarget.Values;

    public TownComp_Spells(Town town) : base(town)
    {
        var allspells = Def.Get<SpellDef>();
        var validspells = allspells.Where(spell => (spell.Subject & SpellSubject.Other) == SpellSubject.Other);
        foreach (var spell in validspells)
            this._spellsOffered.Add(spell, new(spell, price: spell.ManaCost, enabled: true));
    }

    internal override void Tick()
    {
        foreach (var req in this._pendingRequestsByTarget.Values.ToArray())
        {
            if (!req.IsDisposed)
                continue;
            this._pendingRequestsByTarget.Remove(req.Customer);
            this._acceptedRequestsByCaster.Remove(req.Vendor);
            this.Town.ServiceRequests.Remove(req.Id);
            this.Map.Events.Post(new TownServiceCompleteEvent(this.Map, req));
        }
    }
    internal bool TryGetRequestByTarget(Actor target, out ServiceRequest_Spell existing)
        => this._pendingRequestsByTarget.TryGetValue(target.RefId, out existing);

    internal bool TryGetRequestByCaster(Actor caster, out ServiceRequest_Spell existing)
        => this._acceptedRequestsByCaster.TryGetValue(caster.RefId, out existing);

    internal ServiceRequest_Spell GetRequestbyTargetOrDefault(Actor target)
    {
        if (this._pendingRequestsByTarget.TryGetValue(target.RefId, out var existing))
            return existing;
        return null;
    }

    internal ServiceRequest_Spell GetRequestbyCasterOrDefault(Actor caster)
    {
        if (this._acceptedRequestsByCaster.TryGetValue(caster.RefId, out var existing))
            return existing;
        return null;
    }

    internal ServiceRequest_Spell Request(Actor target, SpellDef spell)
    {
        var request = new ServiceRequest_Spell(target, spell, this.GetPrice(spell));// this.PriceList[spell]);
        var id = this.Town.ServiceRequests.Register(request);
        this._pendingRequestsByTarget.Add(target.RefId, request);
        $"spell request created {target.LabelReadable} {spell.LabelReadable}".ToConsole();
        return request;
    }

    void AddInt(ServiceRequest_Spell req)
    {
        this._pendingRequestsByTarget.Add(req.Customer, req);
        if (req.Vendor != EntityRefId.Null)
            this._acceptedRequestsByCaster.Add(req.Vendor, req);
    }

    internal void MarkAccepted(ServiceRequest_Spell req, Actor caster)
    {
        req.AssignVendor(caster);
        if (req.RequiresPayment)
        {
            var spellTarget = this.World.Get<Actor>(req.Customer);
            var payment = this.Town.Trades.Request(spellTarget, caster);
            req.PaymentId = payment.Id;
        }
        this._acceptedRequestsByCaster.Add(req.Vendor, req);
    }

    internal void MarkSucceeded(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkSucceeded();
    }

    internal void MarkPaid(ServiceRequest_Spell req, Actor caster)
    {
        req.MarkPaidFor();
    }

    internal void MarkTargetReady(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkTargetReady();
    }

    internal void MarkCasterReady(Actor caster)
    {
        var req = this._acceptedRequestsByCaster[caster.RefId];
        req.MarkCasterReady();
    }

    internal override void ResolveReferences()
    {
        foreach(var req in this.Town.ServiceRequests.GetAllRequests<ServiceRequest_Spell>())
            this.AddInt(req);
    }

    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        yield return (()=>"Spells", () => UIManager.ToggleSingleton<Gui_TownSpellList>("Spells price list"));
    }

    internal void Request(Actor actor, object spell)
    {
        throw new NotImplementedException();
    }
}
