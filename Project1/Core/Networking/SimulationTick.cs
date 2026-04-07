namespace Project1.Core.Networking;

//public record struct SimulationTick(double Value)
//{
//    public static readonly SimulationTick Immediate = new() { Value = -1 };
//    public static implicit operator double(SimulationTick t) => t.Value;
//    public static implicit operator SimulationTick(double i) => new() { Value = i };
//}
public record struct SimulationTick(ulong Value)
{
    //public static readonly SimulationTick Immediate = new() { Value = -1 };
    public static implicit operator ulong(SimulationTick t) => t.Value;
    public static implicit operator SimulationTick(ulong i) => new() { Value = i };
}