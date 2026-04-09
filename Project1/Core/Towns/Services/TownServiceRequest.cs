using Project1.Core.Simulation;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Towns.Services;

public abstract class TownServiceRequest
{
    internal TownServiceRequestId Id { get; set; }
    abstract internal SimulationTick TickStarted { get; set; }
    //set; }

    abstract internal EntityRefId Buyer { get; }
    abstract internal EntityRefId Seller { get; }
    abstract internal TownServiceDef Service { get; }
    abstract internal int PatienceInitial { get; set; }
    abstract internal bool IsSucceeded { get; }
    abstract internal bool IsFailed { get;}
    abstract internal void Write(IDataWriter w);
    abstract internal void Read(IDataReader r);
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
