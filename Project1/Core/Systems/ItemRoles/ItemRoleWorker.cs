using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

public abstract class ItemRoleWorker
{
    public ItemRoleWorker() { }
    abstract public int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context);
    abstract public int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context);
    abstract public IEnumerable<Def> GetValidTargetDefs();

    //abstract public IEnumerable<ItemRoleDef> GenerateRoles();
}
