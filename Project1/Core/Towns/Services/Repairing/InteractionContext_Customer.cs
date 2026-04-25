using Project1.Core.Interactions;
using Project1.Core.Resources;
using System;

namespace Project1.Core.Towns.Services.Repairing
{
    sealed class InteractionContext_Customer : InteractionContext
    {
        internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
        internal override float GetPercentage(Interaction i)
            => this.Patience.Percentage;
    }
}
