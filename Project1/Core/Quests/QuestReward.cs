using Project1.Framework.Entities.Actors;

namespace Project1.Core.Quests
{
    public abstract class QuestReward
    {
        public QuestDef Parent;
        public QuestReward(QuestDef parent)
        {
            this.Parent = parent;
        }
        public abstract string Text { get; }
        public abstract string Label { get; }
        public abstract int Budget { get; }
        public abstract int Count { get; set; }

        internal abstract void Award(Actor actor);
        internal abstract bool CanAward();
    }
}
