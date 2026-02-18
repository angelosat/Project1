using Project1.Core.Entities;
using Project1.Core.UI;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input
{
    internal class SelectionFinal
    {
        internal HashSet<ISelectable> Targets = [];
        internal HashSet<IntVec3> Cells = [];
        internal IntVec3? Begin, End;

        internal SelectionIntent ToSelectionIntent()
        {
            if(this.Begin.HasValue)
            {
                return new(this.Begin.Value, this.End.Value);
            }
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
                this.SetBox(cell.Global, cell.Global);
            this.Targets.Add(selectable);
        }
        internal void SetBox(IntVec3 begin, IntVec3 end)
        {
            this.Begin = begin;
            this.End = end;
            this.Cells = [.. IntVec3Helper.GetBox(begin, end)];
        }
    }
}
