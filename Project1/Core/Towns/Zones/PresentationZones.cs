using Project1.Core.Systems.Presentation;
using Project1.Core.UI;
using Project1.Framework.UI;


namespace Project1.Core.Towns.Zones;

internal sealed class PresentationZones : IPresentationWorker
{
    public void Register()
    {
        Registry.MapEventHooksClient.Register<ZoneCreatedEvent>(OnZoneCreated);
        Registry.MapEventHooksClient.Register<ZoneDeletedEvent>(OnZoneDeleted);
    }

    private static void OnZoneDeleted(ZoneDeletedEvent e)
    {
        var zone = e.Zone;
        FloatingText.Create(zone.Map, zone.Average(), $"{zone.GetType()} deleted", ft => ft.Font = UIManager.FontBold);
    }
    private static void OnZoneCreated(ZoneCreatedEvent e)
    {
        var zone = e.Zone;
        FloatingText.Create(zone.Map, zone.Average(), $"{zone.GetType()} created", ft => ft.Font = UIManager.FontBold);
    }
}
