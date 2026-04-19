using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using System.Collections.Generic;

namespace Project1.Core.Systems.ItemRoles;

abstract class ItemRoleWorker
{
    public ItemRoleWorker()
    {

    }
    /// <summary>
    /// returns -1 for completely invalid items
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    abstract public int GetInventoryScore(Actor actor, Entity item, ItemRoleDef context);
    //abstract public int GetInventoryScore(Actor actor, Entity item, ItemRoleKey key);
    abstract public int GetSituationalScore(Actor actor, Entity item, ItemRoleDef context);
    //abstract public int GetSituationalScore(Actor actor, Entity item, ItemRoleKey key);
    //abstract public IEnumerable<ItemRoleKey> GenerateKeys();
    //abstract public IEnumerable<ItemRoleDef> GenerateDefs();
    abstract public IEnumerable<Def> GetValidTargetDefs();
    //public Entity FindBest(Actor actor, IEnumerable<Entity> items)
    //{
    //    Entity bestItem = null;
    //    int bestScore = 0;
    //    foreach (var i in items)
    //    {
    //        var score = this.Score(actor, i);
    //        if (score < 0)
    //            continue;
    //        else if (score > bestScore)
    //        {
    //            bestItem = i;
    //            bestScore = score;
    //        }
    //    }
    //    return bestItem;
    //}
}
