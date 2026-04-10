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
    internal TownServiceRequestId Id { get; set; }
    public SimulationTick TickStarted { get; private set; }
    internal int PatienceInitial { get; private set; }

    internal EntityRefId Customer { get; set; }
    internal EntityRefId Vendor { get; set; }
    abstract internal TownServiceDef Service { get; }
    abstract internal bool IsSucceeded { get; }
    abstract internal bool IsFailed { get;}

    protected ServiceRequest(Actor customer)
    {
        this.Customer = customer.RefId;
        this.TickStarted = customer.World.CurrentTick;
        this.PatienceInitial = (int)customer.Resources.GetValue(ResourceDefOf.Patience);
    }
    protected ServiceRequest()
    {
        
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Def", this.Service);
        tag.Save("Id", this.Id);
        tag.Save("TickStarted", this.TickStarted);
        tag.Save("PatienceInitial", this.PatienceInitial);
        tag.Save("Customer", this.Customer);
        tag.Save("Vendor", this.Vendor);
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
        runtime.Customer = tag.LoadEntityRefId("Customer");
        runtime.Vendor = tag.LoadEntityRefId("Vendor");
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

    public ServiceRequest Read(IDataReader r)
    {
        _ = r.ReadDef<TownServiceDef>();

        this.Id = r.ReadUInt64();
        this.TickStarted = r.ReadUInt64();
        this.PatienceInitial = r.ReadInt32();
        this.ReadExtra(r);
        return this;
    }

    public void Write(IDataWriter w)
    {
        w.Write(this.Service);

        w.Write(this.Id);
        w.Write(this.TickStarted);
        w.Write(this.PatienceInitial);
        this.WriteExtra(w);
    }

    protected virtual void SaveExtra(SaveTag tag) { }
    protected virtual void LoadExtra(SaveTag tag) { }
    protected virtual void WriteExtra(IDataWriter w) { }
    protected virtual void ReadExtra(IDataReader r) { }
}

//public interface ITownServiceRequest
//{
//    TownServiceRequestId Id { get; internal set; }
//    EntityRefId Buyer { get; }
//    EntityRefId Seller { get; }
//    TownServiceDef Service { get; }
//    double TickStarted { get; }
//    int PatienceInitial { get; }
//    bool IsSucceeded { get; }
//    bool IsFailed { get; }
//    void Write(IDataWriter w);
//    void Read(IDataReader r);
//}
