//using Project1.Core.Entities;
//using Project1.Core.Entities.Actors;
//using Project1.Core.Helpers;
//using Project1.Core.Networking;
//using Project1.Framework;

//namespace Project1.Core.Towns.Services.Shops
//{
//    [EnsureStaticCtorCall]
//    internal static class PacketsShopTransaction
//    {
//        internal static readonly PacketId
//            _pTransactionUpdated = Registry.PacketHandlers.Register(ReceiveTransactionUpdated),
//            _pTransactionStarted = Registry.PacketHandlers.Register(ReceiveTransactionStarted);

//        static PacketsShopTransaction()
//        {
//            Registry.MapEventHooksServer.Register<ShopTransactionUpdatedEvent>(HandleShopTransactionUpdated);
//            Registry.MapEventHooksServer.Register<TransactionStartedEvent>(HandleTransactionStarted);
//        }

//        private static void HandleTransactionStarted(TransactionStartedEvent e)
//        {
//            SendTransactionStartedEvent(e.Map.Net, e);
//        }
//        static void SendTransactionStartedEvent(NetEndpoint endpoint, TransactionStartedEvent e)
//        {
//            var w = endpoint.BeginPacket(_pTransactionStarted);
//            var transaction = e.Transaction;
//            w.Write(transaction.Customer);
//            w.Write(transaction.Item);
//            w.Write(transaction.Price);
//            w.Write(transaction.Counter);
//        }
//        private static void ReceiveTransactionStarted(NetEndpoint endpoint, Packet packet)
//        {
//            var r = packet.PacketReader;
//            var buyerId = r.ReadEntityRefId();
//            var buyer = endpoint.World.Get<Actor>(buyerId);
//            var itemId = r.ReadEntityRefId();
//            var item = endpoint.World.Get<Entity>(itemId);
//            var price = r.ReadInt32();
//            var counter = r.ReadIntVec3();
//            buyer.Map.Town.Shops.TryBeginTransaction(buyer, item, price, counter, out _);
//        }

//        private static void HandleShopTransactionUpdated(ShopTransactionUpdatedEvent e)
//        {
//            SendTransactionUpdatedEvent(e.Map.Net, e);
//        }
//        static void SendTransactionUpdatedEvent(NetEndpoint endpoint, ShopTransactionUpdatedEvent e)
//        {
//            var w = endpoint.BeginPacket(_pTransactionUpdated);
//            w.Write(e.Transaction.Customer);
//            e.Transaction.Write(w);
//        }

//        private static void ReceiveTransactionUpdated(NetEndpoint endpoint, Packet packet)
//        {
//            var r = packet.PacketReader;
//            var buyerId = r.ReadEntityRefId();
//            var buyer = endpoint.World.Get<Actor>(buyerId);
//            if (!buyer.Map.Town.OpenTransactions.TryGetValue(buyer.RefId, out var transaction))
//                    throw new System.Exception();
//            throw new System.Exception();

//            //transaction.ReadExtra(r);
//        }
//    }
//}
