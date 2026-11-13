using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.ItemRoles;

public class ItemRoleContextDef(string name, Type targetType, Type workerType, Type contextWorkerType) : Def(name)
{
    [Obsolete]
    internal Type TargetType = targetType;
    internal Type WorkerType = workerType;
    internal ItemRoleWorker WorkerRole = ActivatorSafe<ItemRoleWorker>.CreateInstance(workerType);
    internal ItemRoleContextWorker WorkerContext => field ??= ActivatorSafe<ItemRoleContextWorker>.CreateInstance(contextWorkerType);
    //internal ItemRoleGenerator Gen;
}

internal abstract class ItemRoleContextWorker
{
    internal abstract IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef);

    protected IEnumerable<ItemRoleDef> GenerateRolesSimple<T>(ItemRoleContextDef contextDef, Type roleWorkerType) where T : Def
    {
        foreach (var def in Def.Get<T>())
        {
            yield return new ItemRoleDef($"{contextDef.LabelReadable}:{def.LabelReadable}", contextDef, null)
            { Worker = (ItemRoleWorker)Activator.CreateInstance(roleWorkerType) };
        }
    }
    protected IEnumerable<ItemRoleDef> GenerateRolesSimple<T>(Func<T, bool> filter, ItemRoleContextDef contextDef, Type roleWorkerType) where T : Def
    {
        foreach (var def in Def.Get<T>().Where(filter))
        {
            yield return new ItemRoleDef($"{contextDef.LabelReadable}:{def.LabelReadable}", contextDef, null)
            { Worker = (ItemRoleWorker)Activator.CreateInstance(roleWorkerType) };
        }
    }
    protected IEnumerable<ItemRoleDef> GenerateAtomic(ItemRoleContextDef contextDef, Type roleWorkerType)
    {
        yield return new ItemRoleDef($"{contextDef.LabelReadable}", contextDef, null)
        { Worker = (ItemRoleWorker)Activator.CreateInstance(roleWorkerType) };
    }
}

internal sealed class ItemRoleContextWorkerSimple(Type targetDefType) : ItemRoleContextWorker
{
    Type
        TargetDefType = targetDefType;//,
        //RoleWorkerType = roleWorkerType;
    internal override IEnumerable<ItemRoleDef> GenerateRoles(ItemRoleContextDef contextDef)
    {
        var defs = Def.Get(this.TargetDefType);
        foreach (var def in defs)
            yield return new ItemRoleDef($"{contextDef.LabelReadable}:{def.LabelReadable}", contextDef, null) 
            { 
                Worker = ActivatorSafe<ItemRoleWorker>.CreateInstance(contextDef.WorkerType) 
            };
    }
}
