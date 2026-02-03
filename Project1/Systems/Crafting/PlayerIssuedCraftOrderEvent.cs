using Project1.Framework.Base;

namespace Start_a_Town_
{
    internal record struct PlayerIssuedCraftOrderEvent(BlockWorkstationComp Workstation, Def Craftable) : IEventPayload { }
    internal record struct PlayerIssuedCraftOrderEventNew(BlockWorkstationComp Workstation, AddOrderRequest request) : IEventPayload { }
    public record struct PlayerId(int Value)
    {
        public static implicit operator int(PlayerId v) => (int)v;
        public static implicit operator PlayerId(int v) => new(v);

    }
}