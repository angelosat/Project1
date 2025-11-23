using System.Collections.Generic;

namespace Start_a_Town_
{
    abstract class ItemRole
    {
        public abstract IItemPreferenceContext Context { get; }
        public ItemRole()
        {

        }
        /// <summary>
        /// returns -1 for completely invalid items
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        abstract public int Score(Actor actor, Entity item);
        public Entity FindBest(Actor actor, IEnumerable<Entity> items)
        {
            Entity bestItem = null;
            int bestScore = 0;
            foreach (var i in items)
            {
                var score = this.Score(actor, i);
                if (score < 0)
                    continue;
                else if (score > bestScore)
                {
                    bestItem = i;
                    bestScore = score;
                }
            }
            return bestItem;
        }
    }
}
