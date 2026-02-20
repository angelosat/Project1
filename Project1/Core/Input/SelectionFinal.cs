using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input
{
    internal class SelectionFinal
    {
        internal HashSet<ISelectable> Targets = [];
        internal IntVec3? Begin, End;
        internal IEnumerable<IntVec3> Cells
            => this.Targets.OfType<CellSelection>().Select(c => c.Global);
        internal IReadOnlyCollection<Entity> Entities
            => [.. this.Targets.OfType<Entity>()];
        internal Zone Zone => this.Targets.Count == 1 && this.Targets.Single() is CellSelection cell ? cell.Map.Town.GetZoneAt(cell.Global) : null;
        internal SelectionIntent ToSelectionIntent()
        {
            if(this.Begin.HasValue)
                return new(this.Begin.Value, this.End.Value);
            return new(this.Targets.Select(t => (EntityRefId)(t as Entity).RefId));
        }
        internal void Clear()
        {
            this.Targets.Clear();
            this.Begin = null;
            this.End = null;
        }
        internal void Add(ISelectable selectable)
        {
            if (selectable is CellSelection cell)
                this.SetBox(cell.Map, cell.Global, cell.Global);
            this.Targets.Add(selectable);
        }
        internal void SetBox(MapBase map, IntVec3 begin, IntVec3 end)
        {
            this.Begin = begin;
            this.End = end;
            //this.Cells = [.. IntVec3Helper.GetBox(begin, end)];
            this.Targets.Clear();
            this.Targets = [.. IntVec3Helper.GetBox(begin, end)
                .Where(c => !map.IsAir(c))
                .Select(c => new CellSelection(map, c))];
        }
    }
}
