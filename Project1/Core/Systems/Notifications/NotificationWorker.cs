using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Towns.Duties;
using Project1.Core.Towns.Shops;

namespace Project1.Core.Systems.Notifications
{
    public abstract class NotificationWorker
    {
        public abstract void Hook();
    }
    public sealed class NotificationNoWorkerAssigned : NotificationWorker
    {
        public override void Hook()
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
}
