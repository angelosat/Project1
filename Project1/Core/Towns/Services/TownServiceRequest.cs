using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Towns.Services;

public abstract class TownServiceRequest : ISaveableNewNew<TownServiceRequest>, ISerializableNew<TownServiceRequest>
{
    internal TownServiceRequestId Id { get; set; }
    public SimulationTick TickStarted { get; private set; }
    internal int PatienceInitial { get; private set; }

    abstract internal EntityRefId Buyer { get; }
    abstract internal EntityRefId Seller { get; }
    abstract internal TownServiceDef Service { get; }
    //abstract internal int PatienceInitial { get; set; }
    abstract internal bool IsSucceeded { get; }
    abstract internal bool IsFailed { get;}

    protected TownServiceRequest(Actor customer)//SimulationTick tickStarted, int initialPatience)
    {
        //this.TickStarted = tickStarted;
        //this.PatienceInitial = initialPatience;
        this.TickStarted = customer.World.CurrentTick;
        this.PatienceInitial = (int)customer.Resources.GetValue(ResourceDefOf.Patience);
    }
    protected TownServiceRequest()
    {
        
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Def", this.Service);
        tag.Save("Id", this.Id);
        tag.Save("TickStarted", this.TickStarted);
        tag.Save("PatienceInitial", this.PatienceInitial);
        this.SaveExtra(tag);
        return tag;
    }

    public static TownServiceRequest Create(SaveTag tag)
    {
        var def = tag.LoadDef<TownServiceDef>("Def");
        var runtime = def.Worker.CreateRuntime();
        //runtime.Id = tag.Load<TownServiceRequestId>("Id");
        //runtime.TickStarted = tag.Load<SimulationTick>("TickStarted");
        //runtime.PatienceInitial = tag.Load<int>("PatienceInitial");
        runtime.Id = (TownServiceRequestId)tag.LoadUlong("Id");
        runtime.TickStarted = (SimulationTick)tag.LoadUlong("TickStarted");
        runtime.PatienceInitial = tag.LoadInt("PatienceInitial");
        runtime.LoadExtra(tag);
        return runtime;
    }

    //public IDataWriter Write(IDataWriter w)
    //{
    //    w.Write(this.Service);
    //    w.Write(this.Id);
    //    w.Write(this.TickStarted);
    //    w.Write(this.PatienceInitial);
    //    this.WriteExtra(w);
    //    return w;
    //}

    public static TownServiceRequest Create(IDataReader r)
    {
        var def = r.ReadDef<TownServiceDef>();
        var runtime = def.Worker.CreateRuntime();
        runtime.Read(r);
        //runtime.Id = r.ReadUInt64();
        //runtime.TickStarted = r.ReadUInt64();
        //runtime.PatienceInitial = r.ReadInt32();
        //runtime.ReadExtra(r);
        return runtime;
    }
    protected virtual void SaveExtra(SaveTag tag) { }
    protected virtual void LoadExtra(SaveTag tag) { }
    protected virtual void WriteExtra(IDataWriter w) { }
    protected virtual void ReadExtra(IDataReader r) { }

    public TownServiceRequest Read(IDataReader r)
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
