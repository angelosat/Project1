using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Systems.Presentation;
using Project1.Core.Towns.Duties;

namespace Project1.Core.Towns.Services.Shops;

public sealed class PresentationTransactions : IPresentationWorker
{
    public void Register()
    {
        Registry.MapEventHooksClient.Register<TransactionStartedEvent>(HandleTransactionStarted);
    }

    private void HandleTransactionStarted(TransactionStartedEvent e)
    {
        var map = e.Map;
        if (map.Town.DutiesManager.HasAssigned(DutyDefOf.Cashier))
            return;
        Ingame.Net.ChatService.Post(ChatSource.System, "No cashier assigned");
    }
}
