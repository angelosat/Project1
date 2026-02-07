using Project1.Core.Base;

namespace Project1.Core.Towns.Crafting
{
    internal record struct PlayerIssuedCraftOrderEvent(BlockWorkstationComp Workstation, Def Craftable) : IEventPayload { }
    internal record struct PlayerIssuedCraftOrderEventNew(BlockWorkstationComp Workstation, AddOrderRequest request) : IEventPayload { }
    public record struct PlayerId(int Value)
    {
        public static implicit operator int(PlayerId v) => (int)v;
        public static implicit operator PlayerId(int v) => new(v);

    }
}