namespace Project1.Core.Networking;

//public record struct SimulationTick(double Value)
//{
//    public static readonly SimulationTick Immediate = new() { Value = -1 };
//    public static implicit operator double(SimulationTick t) => t.Value;
//    public static implicit operator SimulationTick(double i) => new() { Value = i };
//}
public record struct SimulationTick(long Value)
{
    public static readonly SimulationTick Immediate = new() { Value = -1 };
    public static implicit operator long(SimulationTick t) => t.Value;
    public static implicit operator SimulationTick(long i) => new() { Value = i };
}