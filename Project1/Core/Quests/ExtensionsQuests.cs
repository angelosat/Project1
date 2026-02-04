using Project1.Core.Quests;

namespace Project1.Core.Towns
{
    static class ExtensionsQuests
    {
        static public QuestDef GetQuest(this Town town, int questID)
        {
            return town.QuestManager.GetQuest(questID);
        }
    }
}
