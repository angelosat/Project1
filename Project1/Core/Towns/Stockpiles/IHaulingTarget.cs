using Project1.Core.Entities;
using Project1.Core.Legacy.Storage;
using Project1.Core.Towns.Storage;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Towns.Stockpiles;

public interface IHaulingTarget
{
    StoragePriority Priority { get; }
    IReadOnlyList<Entity> Items { get; }

    IEnumerable<IntVec3> GetCandidateCells(Entity item);
    int GetAvailableSpaceFor(Entity item);
    bool Accepts(Entity item);
    bool Contains(Entity item);
}
sealed record StockpileHaulingTarget(Stockpile Stockpile) : IHaulingTarget
{
    public StoragePriority Priority => this.Stockpile.Priority;

    public IReadOnlyList<Entity> Items => this.Stockpile.Items;

    public bool Accepts(Entity item) => this.Stockpile.Accepts(item);

    public bool Contains(Entity item) => this.Stockpile.Contains(item);

    public int GetAvailableSpaceFor(Entity item) => this.Stockpile.AvailableCapacityFor(item);

    public IEnumerable<IntVec3> GetCandidateCells(Entity item) => this.Stockpile.FindPlacesFor(item);
}
sealed record BlockInventoryHaulingTarget(BlockInventoryComp Comp) : IHaulingTarget
{
    public StoragePriority Priority => this.Comp.Priority;

    public IReadOnlyList<Entity> Items => this.Comp.Items;

    public bool Accepts(Entity item) => this.Comp.Accepts(item);

    public bool Contains(Entity item) => this.Comp.Contains(item);

    public int GetAvailableSpaceFor(Entity item) => this.Comp.AvailableCapacityFor(item);

    public IEnumerable<IntVec3> GetCandidateCells(Entity item) => [this.Comp.Parent.OriginGlobal];
}
