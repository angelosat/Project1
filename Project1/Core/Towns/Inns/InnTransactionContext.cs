using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using System.Collections.Generic;

namespace Project1.Core.Towns.Inns
{
    sealed class InnTransactionContext : InteractionContext
    {
        internal InnManager Manager => field ??= this.Actor.Map.Town.InnManager;
        internal Queue<Actor> Queue => field ??= this.Manager.GetQueue(this.Target.Global);
        internal Actor NextGuest => field ??= this.Queue.Peek();
        internal ServiceRequest_Inn Transaction => field ??= this.Manager.GetTransactionByGuest(this.NextGuest);
    }
}
