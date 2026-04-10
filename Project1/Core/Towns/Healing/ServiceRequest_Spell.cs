using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Magic;
using Project1.Core.Towns.Services;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Towns.Healing;

public sealed class ServiceRequest_Spell : ServiceRequest
{
    enum States { Requested, Accepted, Succeeded, Failed }
    States State;
    internal int Price { get; private set; }
    internal SpellDef Spell;

    public ServiceRequest_Spell(Actor actor, SpellDef spell, int price) : base(actor)
    {
        Price = price;
        Spell = spell;
    }

    public ServiceRequest_Spell()
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

    internal override TownServiceDef Service => TownServiceDefOf.Healing;

    internal override bool IsSucceeded => this.State == States.Succeeded;

    internal override bool IsFailed => this.State == States.Failed;

    public bool RequiresPayment => this.Price > 0;

    public ulong PaymentId { get; internal set; }

    internal void MarkAccepted(Actor caster)
    {
        if (this.State != States.Requested)
            throw new InvalidOperationException();
        this.Vendor = caster.RefId;
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

    protected override void SaveExtra(SaveTag tag)
    {
        tag.Save("Spell", this.Spell);
        tag.Save("Price", this.Price);
        tag.Save("IsPaid", this.IsPaid);
        tag.Save("IsTargetReady", this.IsTargetReady);
        tag.Save("IsCasterReady", this.IsCasterReady);
        tag.Save("State", (int)this.State);
    }

    protected override void LoadExtra(SaveTag tag)
    {
        this.Spell = tag.LoadDef<SpellDef>("Spell");
        this.Price = tag.LoadInt("Price");
        this.IsPaid = tag.LoadBool("IsPaid");
        this.IsTargetReady = tag.LoadBool("IsTargetReady");
        this.IsCasterReady = tag.LoadBool("IsCasterReady");
        this.State = (States)tag.LoadInt("State");
    }

    protected override void WriteExtra(IDataWriter w)
    {
        w.Write(this.Spell);
        w.Write(this.Price);
        w.Write(this.IsPaid);
        w.Write(this.IsTargetReady);
        w.Write(this.IsCasterReady);
        w.Write((int)this.State);
    }

    protected override void ReadExtra(IDataReader r)
    {
        this.Spell = r.ReadDef<SpellDef>();
        this.Price = r.ReadInt32();
        this.IsPaid = r.ReadBoolean();
        this.IsTargetReady = r.ReadBoolean();
        this.IsCasterReady = r.ReadBoolean();
        this.State = (States)r.ReadInt32();
    }
}
