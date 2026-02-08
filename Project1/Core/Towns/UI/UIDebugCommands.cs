using System.Collections.Generic;
using System.Linq;
using Project1.Core.Components.Plants;
using Project1.Core.Net;
using Project1.Core.UI.Hud;
using Project1.Framework.UI;

namespace Project1.Core.Towns.UI
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
                new Button("Grow selected") { LeftClickAction = () => GrowPlants(SelectionManager.GetSelectedEntities().Select(t => t.RefId)) }
                );
        }

        private void GrowPlants(IEnumerable<int> enumerable)
        {
            foreach (var id in enumerable)
            {
                var plant = Server.Instance.World.GetEntity(id);
                plant.TryGetComponent<PlantComponent>(c => c.FinishGrowing(plant));
                plant.TryGetComponent<TreeComponent>(c => c.FinishGrowing(plant));
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
