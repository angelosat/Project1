using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input
{
    public record struct SelectionResolved(List<InteractionTarget> Targets, SelectionIntent Source)
    {
        public SelectionResolved(IEnumerable<InteractionTarget> targets, SelectionIntent source)
            : this(targets.ToList(), source)
        {

        }
    }
}
