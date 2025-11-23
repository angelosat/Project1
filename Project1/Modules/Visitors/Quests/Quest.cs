namespace Start_a_Town_
{
    class Quest
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
