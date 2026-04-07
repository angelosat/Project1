using Project1.Core.UI;

namespace Project1.Core.Towns
{
    class TownsManager : GameSystem
    {
        public override void OnContextActionBarCreated(ContextActionBar.ContextActionBarArgs args)
        {
            foreach (var comp in args.Target.Map.Town.TownComponents)
                comp.OnContextActionBarCreated(args);
        }
    }
}
