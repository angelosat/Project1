using System;

namespace Start_a_Town_
{
    public class ItemRoleDef(ItemRoleContextDef context, Def specific) : Def($"ItemRole:{context.Name}:{specific.Name}")
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
