using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Services;
using Project1.Core.Towns.Shops;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Healing;

sealed class SpellRequest(Actor actor, SpellDef spell, int price) : ITownServiceTransaction
{
    enum States { Requested, Accepted, WaitingPay, Paid, CasterReady, TargetReady, Succeeded, Failed }
    States State;
    internal EntityRefId TargetId = actor.RefId;
    internal EntityRefId CasterId;
    internal int Price { get; private set; } = price;
    internal SpellDef Spell = spell;

    internal bool IsPending => this.State == States.Requested;
    internal bool IsWaitingPay => this.State == States.WaitingPay;
    internal bool IsPaid => this.State == States.Paid;
    internal bool IsAccepted => this.State == States.Accepted;
    internal bool IsTargetReady => this.State == States.TargetReady;
    internal bool IsCasterReady => this.State == States.CasterReady;
    internal bool IsDisposed => this.State == States.Succeeded || this.State == States.Failed;

    public EntityRefId Buyer => this.TargetId;

    public EntityRefId Seller => this.CasterId;

    public TownServiceDef Service => TownServiceDefOf.Healing;

    public double TickStarted { get; set; } = actor.World.CurrentTick;

    public int PatienceInitial { get; set; } = (int)actor.Resources.GetValue(ResourceDefOf.Patience);

    public bool IsSucceeded => this.State == States.Succeeded;

    public bool IsFailed => this.State == States.Failed;


    internal void MarkAccepted(Actor caster)
    {
        if (this.State != States.Requested)
            throw new InvalidOperationException();
        this.CasterId = caster.RefId;
        if (this.Price > 0)
            this.State = States.WaitingPay;
        else
            this.State = States.Paid;// States.Accepted;
    }

    internal void MarkTargetReady()
    {
        if (this.State != States.CasterReady)
            throw new InvalidOperationException();
        this.State = States.TargetReady;
    }
    internal void MarkCasterReady()
    {
        if (this.State != States.Paid)
            throw new InvalidOperationException();
        this.State = States.CasterReady;
    }
    internal void MarkPaid()
    {
        if (this.State != States.WaitingPay)
            throw new InvalidOperationException();
        this.State = States.Paid;
    }

    internal void MarkSucceeded()
        => this.State = States.Succeeded;

    public void Write(IDataWriter w)
    {
        w.Write(this.TargetId);
        w.Write(this.CasterId);
        w.Write(this.Spell);
        w.Write(this.Price);
        w.Write((int)this.State);
    }

    public void Read(IDataReader r)
    {
        this.TargetId = r.ReadEntityRefId();
        this.CasterId = r.ReadEntityRefId();
        this.Spell = r.ReadDef<SpellDef>();
        this.Price = r.ReadInt32();
        this.State = (States)r.ReadInt32();
    }

    
}

public class TownComp_Spells : TownComp
{
    public override string Name => "Spells";

    readonly Dictionary<EntityRefId, SpellRequest> _pendingRequestsByTarget = [];
    readonly Dictionary<EntityRefId, SpellRequest> _acceptedRequestsByCaster = [];
    internal Dictionary<SpellDef, int> PriceList = new() { { SpellDefOf.Healing, 0 } };// 100 } };

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
        this._pendingRequestsByTarget.Add(target.RefId, request);
        this.Map.Events.Post(new HealingRequestCreatedEvent(target, spell));
        $"spell request created {target.LabelReadable} {spell.LabelReadable}".ToConsole();
        return request;
    }
    internal void MarkAccepted(SpellRequest req, Actor caster)
    {
        req.MarkAccepted(caster);
        this._acceptedRequestsByCaster.Add(req.CasterId, req);
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
    }

    internal void MarkSucceeded(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkSucceeded();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
    }

    internal void MarkPaid(SpellRequest req, Actor caster)
    {
        req.MarkPaid();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
    }

    internal void MarkTargetReady(Actor target)
    {
        var req = this._pendingRequestsByTarget[target.RefId];
        req.MarkTargetReady();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
    }

    internal void MarkCasterReady(Actor caster)
    {
        var req = this._acceptedRequestsByCaster[caster.RefId];
        req.MarkCasterReady();
        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
    }
}


//using Project1.Core.Entities.Actors;
//using Project1.Core.Helpers;
//using Project1.Core.Resources;
//using Project1.Core.Systems.Magic;
//using Project1.Core.Towns.Services;
//using Project1.Core.Towns.Shops;
//using Project1.Framework.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Project1.Core.Towns.Healing;

//sealed class SpellRequest(Actor actor, SpellDef spell) : ITownServiceTransaction
//{
//    enum States { Requested, Accepted, Ready, Succeeded, Failed }
//    States State;
//    internal EntityRefId TargetId = actor.RefId;
//    internal EntityRefId CasterId;
//    internal SpellDef Spell = spell;
//    internal Dictionary<SpellDef, int> PriceList = new() { { SpellDefOf.Healing, 100 } };

//    internal bool IsAccepted => this.State == States.Accepted;
//    internal bool IsReady => this.State == States.Ready;
//    internal bool IsDisposed => this.State == States.Succeeded || this.State == States.Failed;

//    public EntityRefId Buyer => this.TargetId;

//    public EntityRefId Seller => this.CasterId;

//    public TownServiceDef Service => TownServiceDefOf.Healing;

//    public double TickStarted { get; set; } = actor.World.CurrentTick;

//    public int PatienceInitial { get; set; } = (int)actor.Resources.GetValue(ResourceDefOf.Patience);

//    public bool IsSucceeded => this.State == States.Succeeded;

//    public bool IsFailed => this.State == States.Failed;

//    internal void MarkAccepted(Actor caster)
//    {
//        if (this.State != States.Requested)
//            throw new InvalidOperationException();
//        this.CasterId = caster.RefId;
//        this.State = States.Accepted;
//    }

//    internal void MarkReady()
//    {
//        if (this.State != States.Accepted)
//            throw new InvalidOperationException();
//        this.State = States.Ready;
//    }

//    internal void MarkSuceeded()
//        => this.State = States.Succeeded;

//    public void Write(IDataWriter w)
//    {
//        w.Write(this.TargetId);
//        w.Write(this.CasterId);
//        w.Write(this.Spell);
//        w.Write((int)this.State);
//    }

//    public void Read(IDataReader r)
//    {
//        this.TargetId = r.ReadEntityRefId();
//        this.CasterId = r.ReadEntityRefId();
//        this.Spell = r.ReadDef<SpellDef>();
//        this.State = (States)r.ReadInt32();
//    }
//}

//public class TownComp_Spells : TownComp
//{
//    public override string Name => "Spells";

//    readonly Dictionary<EntityRefId, SpellRequest> _pendingRequestsByTarget = [];
//    //readonly Dictionary<EntityRefId, SpellRequest> _acceptedRequestsByTarget = [];
//    readonly Dictionary<EntityRefId, SpellRequest> _acceptedRequestsByCaster = [];

//    public TownComp_Spells(Town town) : base(town)
//    {
//    }

//    internal ICollection<SpellRequest> PendingRequests => this._pendingRequestsByTarget.Values;
//    public override void Tick()
//    {
//        foreach(var req in this._pendingRequestsByTarget.Values.ToArray())
//        {
//            if (!req.IsDisposed)
//                continue;
//            this._pendingRequestsByTarget.Remove(req.TargetId);
//            this._acceptedRequestsByCaster.Remove(req.CasterId);
//            this.Map.Events.Post(new TownServiceComplete(this.Map, req));
//        }
//    }
//    internal bool TryGetRequestByTarget(Actor target, out SpellRequest existing)
//        => this._pendingRequestsByTarget.TryGetValue(target.RefId, out existing);
//    internal bool TryGetRequestByCaster(Actor caster, out SpellRequest existing)
//        => this._acceptedRequestsByCaster.TryGetValue(caster.RefId, out existing);
//    internal SpellRequest GetRequestbyTargetOrDefault(Actor target)
//    {
//        if (this._pendingRequestsByTarget.TryGetValue(target.RefId, out var existing))
//            return existing;
//        return null;
//    }
//    internal SpellRequest GetRequestbyCasterOrDefault(Actor caster)
//    {
//        if (this._acceptedRequestsByCaster.TryGetValue(caster.RefId, out var existing))
//            return existing;
//        return null;
//    }
//    internal SpellRequest Request(Actor target, SpellDef spell)
//    {
//        var request = new SpellRequest(target, spell);
//        this._pendingRequestsByTarget.Add(target.RefId, request);
//        this.Map.Events.Post(new HealingRequestCreatedEvent(target, spell));
//        return request;
//    }
//    //internal SpellRequest GetOrCreateInt(Actor target)
//    //{
//    //    if (this._pendingRequestsByTarget.TryGetValue(target.RefId, out var existing))
//    //        return existing;
//    //    return Request(target);
//    //}

//    internal void MarkAccepted(SpellRequest req, Actor caster)
//    {
//        //req.CasterId = caster.RefId;
//        req.MarkAccepted(caster);
//        //this._pendingRequestsByTarget.Remove(req.TargetId);
//        //this._acceptedRequestsByTarget.Add(req.TargetId, req);
//        this._acceptedRequestsByCaster.Add(req.CasterId, req);
//        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));

//    }

//    internal void MarkSucceeded(Actor target)
//    {
//        var req = this._pendingRequestsByTarget[target.RefId];
//        req.MarkSuceeded();
//        this.Map.Events.Post(new HealingRequestUpdatedEvent(req));
//    }
//}
