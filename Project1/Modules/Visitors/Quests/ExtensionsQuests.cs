namespace Start_a_Town_
{
    static class ExtensionsQuests
    {
        static public QuestDef GetQuest(this Town town, int questID)
        {
            return town.QuestManager.GetQuest(questID);
        }
    }
}
