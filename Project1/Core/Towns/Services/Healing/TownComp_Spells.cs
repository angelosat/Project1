using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Services.Shops;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Healing;

public class TownComp_Spells : TownComp
{
    public override string Name => "Spells";

    readonly Dictionary<EntityRefId, ServiceRequest_Spell> _pendingRequestsByTarget = [];
    readonly Dictionary<EntityRefId, ServiceRequest_Spell> _acceptedRequestsByCaster = [];
    internal Dictionary<SpellDef, int> PriceList = new() { { SpellDefOf.Healing, 100 } };// 100 } };
    internal ICollection<ServiceRequest_Spell> PendingRequests => this._pendingRequestsByTarget.Values;

    public TownComp_Spells(Town town) : base(town)
    {
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
        var request = new ServiceRequest_Spell(target, spell, this.PriceList[spell]);
        var id = this.Town.ServiceRequests.Register(request);
        this._pendingRequestsByTarget.Add(target.RefId, request);
        //this.Map.Events.Post(new HealingRequestCreatedEvent(target, spell));
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
        req.MarkAccepted(caster);
        if (req.RequiresPayment)
        {
            var spellTarget = this.World.Get<Actor>(req.Customer);
            var payment = this.Town.Trades.Request(spellTarget, caster);
            req.PaymentId = payment.Id;
        }
        this._acceptedRequestsByCaster.Add(req.Vendor, req);
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }

    internal void MarkSucceeded(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkSucceeded();
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

    internal void MarkPaid(ServiceRequest_Spell req, Actor caster)
    {
        req.MarkPaid();
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }

    internal void MarkTargetReady(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkTargetReady();
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }

    internal void MarkCasterReady(Actor caster)
    {
        var req = this._acceptedRequestsByCaster[caster.RefId];
        req.MarkCasterReady();
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }

    internal override void ResolveReferences()
    {
        foreach(var req in this.Town.ServiceRequests.GetAllRequests<ServiceRequest_Spell>())
            this.AddInt(req);
    }
}
