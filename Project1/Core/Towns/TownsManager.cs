using Project1.Core.Components;
using Project1.Core.Base;
using Project1.Core.Net;
using Project1.Core.UI;

namespace Project1.Core.Towns
{
    class TownsManager : GameSystem
    {
        public override void Initialize()
        {
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
