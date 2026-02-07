using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles
{
    public class ItemRoleDef(ItemRoleContextDef context, Def specific) : Def($"{context.Label}:{specific.Label}")// Def($"ItemRole:{context.Name}:{specific.Name}")
    {
        internal readonly ItemRoleWorker Worker = Activator.CreateInstance(context.WorkerType) as ItemRoleWorker;
        internal readonly ItemRoleContextDef Context = context;
        internal readonly Def Def = specific;

        internal int GetSituationalScore(Actor actor, Entity item)
        {
            return this.Worker.GetSituationalScore(actor, item, this);
        }
    }
}
