using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class UIDebugCommands : Panel
    {
        static readonly UIDebugCommands Instance;
        static UIDebugCommands()
        {
            Instance = new UIDebugCommands();
            Instance.ToWindow("Debug commands");
        }
        public UIDebugCommands()
        {
            this.AutoSize = true;
            this.AddControlsVertically(
                new Button("Grow selected") { LeftClickAction = () => GrowPlants(SelectionManager.GetSelectedEntities().Select(t=>t.RefId)) }
                );
        }

        private void GrowPlants(IEnumerable<int> enumerable)
        {
            foreach (var id in enumerable)
            {
                var plant = Server.Instance.GetNetworkEntity(id);
                plant.TryGetComponent<Components.PlantComponent>(c => c.FinishGrowing(plant));
                plant.TryGetComponent<Components.TreeComponent>(c => c.FinishGrowing(plant));
                plant.Sync(Server.Instance);
            }
        }
        internal static void RefreshNew()
        {
            var win = Instance.GetWindow();
            win.Show();
            win.Location = UIManager.Mouse;
        }
    }
}
