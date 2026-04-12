using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Towns.Services.Healing;

sealed class InteractionContext_Healing : InteractionContext
{
    internal TownComp_Spells Manager => field ??= this.Actor.Map.Town.Spells;
    internal ServiceRequest_Spell RequestByTarget => field ??= this.Manager.GetRequestbyTargetOrDefault(this.Actor);
    internal ServiceRequest_Spell RequestByCaster => field ??= this.Manager.GetRequestbyCasterOrDefault(this.Actor);
    internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
    internal override float GetPercentage(Interaction i) => this.Patience.Percentage;
}
