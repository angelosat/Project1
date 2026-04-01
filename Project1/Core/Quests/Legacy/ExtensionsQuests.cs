using Project1.Core.Towns;

namespace Project1.Core.Quests.Legacy
{
    static class ExtensionsQuests
    {
        static public QuestDef GetQuest(this Town town, int questID)
        {
            return town.QuestManager.GetQuest(questID);
        }
    }
}
