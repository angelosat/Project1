using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Services;
using Project1.Core.Towns.Shops;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Healing;

public sealed class SpellRequest : TownServiceRequest
{
    enum States { Requested, Accepted, Succeeded, Failed }
    States State;
    internal EntityRefId TargetId;
    internal EntityRefId CasterId;
    internal int Price { get; private set; }
    internal SpellDef Spell;

    public SpellRequest(Actor actor, SpellDef spell, int price) : base(actor)//.World.CurrentTick)
    {
        TargetId = actor.RefId;
        Price = price;
        Spell = spell;
        //TickStarted = actor.World.CurrentTick;
        //PatienceInitial = (int)actor.Resources.GetValue(ResourceDefOf.Patience);
    }

    public SpellRequest()
    {
    }

    internal bool IsPending => this.State == States.Requested;
    internal bool IsPaid 
    {
        get => field;
        set => field |= value;
    }
    internal bool IsAccepted => this.State == States.Accepted;
    internal bool IsTargetReady { get; private set; }
    internal bool IsCasterReady { get; private set; }
    internal bool IsDisposed => this.State == States.Succeeded || this.State == States.Failed;

    internal override EntityRefId Buyer => this.TargetId;

    internal override EntityRefId Seller => this.CasterId;

    internal override TownServiceDef Service => TownServiceDefOf.Healing;

    //internal override SimulationTick TickStarted { get; set; }

    //internal override int PatienceInitial { get; set; }

    internal override bool IsSucceeded => this.State == States.Succeeded;

    internal override bool IsFailed => this.State == States.Failed;

    public bool RequiresPayment => this.Price > 0;

    public ulong PaymentId { get; internal set; }

    internal void MarkAccepted(Actor caster)
    {
        if (this.State != States.Requested)
            throw new InvalidOperationException();
        this.CasterId = caster.RefId;
        this.State = States.Accepted;
        if (this.Price == 0)
            this.IsPaid = true;
    }

    internal void MarkTargetReady()
        => this.IsTargetReady = true;

    internal void MarkCasterReady()
        => this.IsCasterReady = true;   
            
    internal void MarkPaid()
        => this.IsPaid = true;

    internal void MarkSucceeded()
        => this.State = States.Succeeded;

    protected override void WriteExtra(IDataWriter w)
    {
        w.Write(this.TargetId);
        w.Write(this.CasterId);
        w.Write(this.Spell);
        w.Write(this.Price);
        w.Write(this.IsPaid);
        w.Write(this.IsTargetReady);
        w.Write(this.IsCasterReady);
        w.Write((int)this.State);
    }

    protected override void ReadExtra(IDataReader r)
    {
        this.TargetId = r.ReadEntityRefId();
        this.CasterId = r.ReadEntityRefId();
        this.Spell = r.ReadDef<SpellDef>();
        this.Price = r.ReadInt32();
        this.IsPaid = r.ReadBoolean();
        this.IsTargetReady = r.ReadBoolean();
        this.IsCasterReady = r.ReadBoolean();
        this.State = (States)r.ReadInt32();
    }
}

public class TownComp_Spells : TownComp
{
    public override string Name => "Spells";

    readonly Dictionary<EntityRefId, SpellRequest> _pendingRequestsByTarget = [];
    readonly Dictionary<EntityRefId, SpellRequest> _acceptedRequestsByCaster = [];
    internal Dictionary<SpellDef, int> PriceList = new() { { SpellDefOf.Healing, 100 } };// 100 } };

    public TownComp_Spells(Town town) : base(town)
    {
    }

    internal ICollection<SpellRequest> PendingRequests => this._pendingRequestsByTarget.Values;
    public override void Tick()
    {
        foreach (var req in this._pendingRequestsByTarget.Values.ToArray())
        {
            if (!req.IsDisposed)
                continue;
            this._pendingRequestsByTarget.Remove(req.TargetId);
            this._acceptedRequestsByCaster.Remove(req.CasterId);
            this.Town.ServiceRequests.Remove(req.Id);
            this.Map.Events.Post(new TownServiceComplete(this.Map, req));
        }
    }
    internal bool TryGetRequestByTarget(Actor target, out SpellRequest existing)
        => this._pendingRequestsByTarget.TryGetValue(target.RefId, out existing);

    internal bool TryGetRequestByCaster(Actor caster, out SpellRequest existing)
        => this._acceptedRequestsByCaster.TryGetValue(caster.RefId, out existing);

    internal SpellRequest GetRequestbyTargetOrDefault(Actor target)
    {
        if (this._pendingRequestsByTarget.TryGetValue(target.RefId, out var existing))
            return existing;
        return null;
    }

    internal SpellRequest GetRequestbyCasterOrDefault(Actor caster)
    {
        if (this._acceptedRequestsByCaster.TryGetValue(caster.RefId, out var existing))
            return existing;
        return null;
    }

    internal SpellRequest Request(Actor target, SpellDef spell)
    {
        var request = new SpellRequest(target, spell, this.PriceList[spell]);
        var id = this.Town.ServiceRequests.Register(request);
        this._pendingRequestsByTarget.Add(target.RefId, request);
        this.Map.Events.Post(new HealingRequestCreatedEvent(target, spell));
        $"spell request created {target.LabelReadable} {spell.LabelReadable}".ToConsole();
        return request;
    }

    internal void MarkAccepted(SpellRequest req, Actor caster)
    {
        req.MarkAccepted(caster);
        if (req.RequiresPayment)
        {
            var spellTarget = this.World.Get<Actor>(req.TargetId);
            var payment = this.Town.Trades.Request(spellTarget, caster);
            req.PaymentId = payment.Id;
        }
        this._acceptedRequestsByCaster.Add(req.CasterId, req);
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));
    }

    internal void MarkSucceeded(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkSucceeded();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

    internal void MarkPaid(SpellRequest req, Actor caster)
    {
        req.MarkPaid();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

    internal void MarkTargetReady(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkTargetReady();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

    internal void MarkCasterReady(Actor caster)
    {
        var req = this._acceptedRequestsByCaster[caster.RefId];
        req.MarkCasterReady();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
        //this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }
}
