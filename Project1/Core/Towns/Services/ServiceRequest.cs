using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Towns.Services;

public abstract class ServiceRequest : ISaveableNewNew<ServiceRequest>, ISerializableNew<ServiceRequest>
{
    enum States { Pending, VendorWaitingItem, VendorWorking, VendorWaitingPay, VendorIsPaid, Success, Failure }
    private States State;
    internal TownServiceRequestId Id { get; set; }
    public SimulationTick TickStarted { get; private set; }
    internal int PatienceInitial { get; private set; }
    internal int Price { get; private set; }
    internal EntityRefId Money { get; set; }

    internal IntVec3? Counter { get; private set; }

    internal EntityRefId Customer { get; set; }
    internal EntityRefId Vendor { get; set; }
    abstract internal TownServiceDef Service { get; }
    abstract internal bool IsSucceeded { get; }
    abstract internal bool IsFailed { get;}

    public bool IsPaidFor => this.Money != EntityRefId.Null;

    protected ServiceRequest(Actor customer, int price)
    {
        this.Customer = customer.RefId;
        this.TickStarted = customer.World.CurrentTick;
        this.PatienceInitial = (int)customer.Resources.GetValue(ResourceDefOf.Patience);
        this.Price = price;
    }
    protected ServiceRequest()
    {
        
    }

    public ServiceRequest(Actor customer, int price, IntVec3 counter) : this(customer, price)
    {
        this.Counter = counter;
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Def", this.Service);
        tag.Save("Id", this.Id);
        tag.Save("TickStarted", this.TickStarted);
        tag.Save("PatienceInitial", this.PatienceInitial);
        tag.Save("Price", this.Price);
        tag.Save("Customer", this.Customer);
        tag.Save("Vendor", this.Vendor);
        if (this.Counter.HasValue)
            tag.Save("Counter", this.Counter.Value);
        tag.Save("Money", this.Money);
        tag.Save("State", (int)this.State);
        this.SaveExtra(tag);
        return tag;
    }

    public static ServiceRequest Create(SaveTag tag)
    {
        var def = tag.LoadDef<TownServiceDef>("Def");
        var runtime = def.Worker.CreateRuntime();
        runtime.Id = (TownServiceRequestId)tag.LoadUlong("Id");
        runtime.TickStarted = (SimulationTick)tag.LoadUlong("TickStarted");
        runtime.PatienceInitial = tag.LoadInt("PatienceInitial");
        runtime.Price = tag.LoadInt("Price");
        runtime.Customer = tag.LoadEntityRefId("Customer");
        runtime.Vendor = tag.LoadEntityRefId("Vendor");
        if (tag.TryLoadIntVec3("Counter", out var counter)) 
            runtime.Counter = counter;
        runtime.Money = tag.LoadEntityRefId("Money");
        runtime.State = (States)tag.LoadInt("State");
        runtime.LoadExtra(tag);
        return runtime;
    }

    public static ServiceRequest Create(IDataReader r)
    {
        var def = r.ReadDef<TownServiceDef>();
        var runtime = def.Worker.CreateRuntime();
        runtime.Read(r);
        return runtime;
    }

    public void Write(IDataWriter w)
    {
        w.Write(this.Service);

        w.Write(this.Id);
        w.Write(this.TickStarted);
        w.Write(this.PatienceInitial);
        w.Write(this.Price);
        w.Write(this.Customer);
        w.Write(this.Vendor);
        w.Write(this.Counter.HasValue);
        if (this.Counter.HasValue)
            w.Write(this.Counter.Value);
        w.Write(this.Money);
        w.Write((int)this.State);
        this.WriteExtra(w);
    }

    public ServiceRequest Read(IDataReader r)
    {
        _ = r.ReadDef<TownServiceDef>();

        this.Id = r.ReadUInt64();
        this.TickStarted = r.ReadUInt64();
        this.PatienceInitial = r.ReadInt32();
        this.Price = r.ReadInt32();
        this.Customer = r.ReadEntityRefId();
        this.Vendor = r.ReadEntityRefId();
        if (r.ReadBoolean())
            this.Counter = r.ReadIntVec3();
        this.Money = r.ReadEntityRefId();
        this.State = (States)r.ReadInt32();
        this.ReadExtra(r);
        return this;
    }

    protected virtual void SaveExtra(SaveTag tag) { }
    protected virtual void LoadExtra(SaveTag tag) { }
    protected virtual void WriteExtra(IDataWriter w) { }
    protected virtual void ReadExtra(IDataReader r) { }

    public bool IsVendorWaitingItemSubmit => this.State == States.VendorWaitingItem;
    public bool IsVendorWorking => this.State == States.VendorWorking;
    public bool IsVendorWaitingPayment => this.State == States.VendorWaitingPay;

    internal void MarkVendorWaiting()
        => this.State = States.VendorWaitingItem;

    internal void MarkVendorWorking()
        => this.State = States.VendorWorking;

    internal void MarkVendorWaitingPayment()
        => this.State = States.VendorWaitingPay;

    internal void MarkIsPaidFor()
        => this.State = States.VendorIsPaid;
}
