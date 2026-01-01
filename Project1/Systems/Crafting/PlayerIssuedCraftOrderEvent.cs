namespace Start_a_Town_
{
    internal class PlayerIssuedCraftOrderEvent(BlockWorkstationComp workstation, Def craftable) : IEventPayload
    {
        //internal PlayerId Player;
        public readonly BlockWorkstationComp Workstation = workstation;
        public readonly Def Craftable = craftable;
    }
    public record struct PlayerId(int Value)
    {
        public static implicit operator int(PlayerId v) => (int)v;
        public static implicit operator PlayerId(int v) => new(v);

    }
}