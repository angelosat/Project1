using Project1.Framework.Components;
using Project1.Framework.Base;
using Project1.Framework.Net;
using Start_a_Town_.UI;
using Project1.Framework.Interfaces;

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
