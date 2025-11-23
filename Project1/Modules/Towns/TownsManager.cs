using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class TownsManager : GameComponent
    {
        public override void Initialize()
        {
            PopulationManager.Init();
            NpcComponent.Init();
        }

        public override void OnGameEvent(GameEvent e)
        {
            return;
        }

        public override void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs args)
        {
            foreach (var comp in Engine.Map.Town.TownComponents)
                comp.OnContextActionBarCreated(args);
        }
    }
}
