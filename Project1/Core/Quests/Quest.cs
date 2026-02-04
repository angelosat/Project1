using Project1.Framework.Legacy;

namespace Project1.Core.Quests
{
    public class Quest
    {
        public QuestDef QuestGiver;
        QuestObjective[] Requirements;
        ItemMaterialAmount Reward;
       
        public Quest(QuestDef questGiver, QuestObjective[] requirements, ItemMaterialAmount reward)
        {
            QuestGiver = questGiver;
            Requirements = requirements;
            Reward = reward;
        }
    }
}
