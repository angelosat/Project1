using Project1.Core.Screens;
using Project1.Core.Towns.Zones;
using Project1.Core.UI;

namespace Project1.Core.Input.Orders
{
    internal sealed class OrderCommandDeleteZone : CommandWorker
    {
        internal override bool CanIssue(ISelectable target) => target is Zone;
        //protected override void Execute(MapBase map, IEnumerable<ISelectable> targets)
        //    => map.Town.ZoneManager.DeleteZone(targets.OfType<CellSelection>().Select(c=>c.Global).First());
        internal override void Issue(OrderCommandRuntime runtime, SelectionFinal selection)
        {
            if (selection.Zone is Zone zone)
                Ingame.Instance.Events.Post(new PlayerDeletingZoneEvent(zone));
        }
    }
}
