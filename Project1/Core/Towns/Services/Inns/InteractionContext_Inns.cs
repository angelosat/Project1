using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using System.Collections.Generic;

namespace Project1.Core.Towns.Services.Inns
{
    sealed class InteractionContext_Inns : InteractionContext
    {
        internal TownComp_Inns Manager => field ??= this.Actor.Map.Town.Inns;
        internal Queue<Actor> Queue => field ??= this.Manager.GetQueue(this.Target.Global);
        internal Actor NextGuest => field ??= this.Queue.Peek();
        internal ServiceRequest_Inn Transaction => field ??= this.Manager.GetTransactionByGuest(this.NextGuest);
    }
}
