using Project1.Core.Entities.Actors;
using System;

namespace Project1.Core.Systems.Quests.Legacy
{
    class QuestGiverProperties
    {
        public int Giver { get; private set; }
        public int NextQuestReceiver { get; private set; } = -1;

        public QuestGiverProperties(int giverID)
        {
            this.Giver = giverID;
        }
        internal void HandleReceiver(Actor actor)
        {
            if (this.NextQuestReceiver != -1)
                throw new Exception();
            this.NextQuestReceiver = actor.RefId;
        }
        internal void RemoveReceiver(Actor actor)
        {
            if (this.NextQuestReceiver != actor.RefId)
                throw new Exception();
            this.NextQuestReceiver = -1;
        }
        internal void RemoveReceiver()
        {
            if (this.NextQuestReceiver == -1)
                throw new Exception();
            this.NextQuestReceiver = -1;
        }
        public int GetNextQuestReceiverID()
        {
            return this.NextQuestReceiver;
        }
    }
}
