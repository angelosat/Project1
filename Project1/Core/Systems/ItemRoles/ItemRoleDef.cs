using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Systems.ItemRoles;

public sealed class ItemRoleDef : Def// Def($"ItemRole:{context.Name}:{specific.Name}")
{
    public required ItemRoleWorker Worker;
    internal readonly ItemRoleContextDef Context;
    internal readonly Def Source;

    public ItemRoleDef(ItemRoleContextDef context, Def specific) //: base($"{context.LabelReadable}:{specific?.LabelReadable}")// Def($"ItemRole:{context.Name}:{specific.Name}")
        : this($"{context.LabelReadable}:{specific?.LabelReadable}", context, specific)
    {
        //Worker = Activator.CreateInstance(context.WorkerType) as ItemRoleWorker;
        //Context = context;
        //Source = specific;
    }

    public ItemRoleDef(string name, ItemRoleContextDef context, Def specific)
        : base(name)// Def($"ItemRole:{context.Name}:{specific.Name}")
    {
        //Worker = Activator.CreateInstance(context.WorkerType) as ItemRoleWorker;
        Context = context;
        Source = specific;
    }

    //internal int GetSituationalScore(Actor actor, Entity item)
    //{
    //    return this.Worker.GetSituationalScore(actor, item);
    //}
}
