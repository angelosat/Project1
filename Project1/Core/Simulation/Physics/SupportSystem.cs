using Project1.Framework;
using System.Linq;

namespace Project1.Core.Simulation.Physics;

internal sealed class SupportSystem : SimulationSystem
{
    public SupportSystem(MapBase map) : base(map)
    {
        map.Events.ListenTo<CellsInvalidatedEvent>(OnCellsInvalidated);
    }
    private void OnCellsInvalidated(CellsInvalidatedEvent e)
    {
        foreach (var cell in e.Positions)
        {
            if (e.Map.IsSolid(cell))
                continue;
            var above = cell.Above;
            foreach (var entity in e.Map.GetEntitiesAt(above))
            {
                if (entity.Physics.Enabled)
                    continue;
                if (entity.Physics.CurrentAABB.GetCorners().Where(c => c.Z == above.Z).All(c => c.ToCell() == above))
                    entity.Physics.Enable();
            }
        }
    }
}
