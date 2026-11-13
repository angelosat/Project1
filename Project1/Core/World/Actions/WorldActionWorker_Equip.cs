using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal sealed record WorldActionData_Equip(FrontierRuntime Frontier, Actor Actor) : WorldActionData(Frontier, Actor)
{
    internal required EquipStatusDecision Decision;

    public override void Write(IDataWriter w)
    {
        w.Write(this.Decision.ItemId);
        w.Write(this.Decision.Delta);
    }

    public override ISerializable Read(IDataReader r)
    {
        this.Decision.ItemId = r.ReadEntityRefId();
        this.Decision.Delta = r.ReadInt32();
        return this;
    }
}

internal sealed class WorldActionWorker_Equip : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var typed = (WorldActionData_Equip)data;
        var item = data.Actor.World.Get(typed.Decision.ItemId);
        if (typed.Decision.IsEquip)
            data.Actor.Gear.EquipToggle(item);
        else if (typed.Decision.IsUnequip)
            data.Actor.Inventory.Unequip(item);
    }
}

