namespace Project1.Core.Systems.Quests;

abstract class QuestController
{
    protected TownComp_Quests Comp;
    public void Register(TownComp_Quests comp)
    {
        this.Comp = comp;
        this.OnRegister();
    }
    protected abstract void OnRegister();
}
