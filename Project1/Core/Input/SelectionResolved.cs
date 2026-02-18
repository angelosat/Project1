using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input
{
    public record struct SelectionResolved(List<TargetArgs> Targets, SelectionIntent Source)
    {
        public SelectionResolved(IEnumerable<TargetArgs> targets, SelectionIntent source)
            : this(targets.ToList(), source)
        {

        }
    }
}
