using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketsCrafting
    {
        readonly static int _pPlayerCreatedOrder, _pPlayerDeletedOrder, _pPlayerModifiedOrder, _pOrderUpdated, _pPlayerModifiedOrderFilters;
        static PacketsCrafting()
        {
            _pPlayerCreatedOrder = Registry.PacketHandlers.Register(OnPlayerCreatedOrder);
            _pPlayerDeletedOrder = Registry.PacketHandlers.Register(OnPlayerDeletedOrder);
            _pPlayerModifiedOrder = Registry.PacketHandlers.Register(OnPlayerModifiedOrder);
            _pPlayerModifiedOrderFilters = Registry.PacketHandlers.Register(OnPlayerModifiedOrderFilters);
            _pOrderUpdated = Registry.PacketHandlers.Register(OnCraftOrderUpdated);
            Registry.PlayerInputEventHooks.Register<PlayerIssuedCraftOrderEvent>(HandlePlayerIssuedCraftOrderEvent);
            Registry.PlayerInputEventHooks.Register<PlayerModifiedOrderFiltersEvent>(HandlePlayerModifiedOrderFilters);
            Registry.MapEventHooksServer.Register<CraftOrderCompletedEvent>(HandleCraftOrderCompletedEvent);
        }
        private static void HandlePlayerModifiedOrderFilters(PlayerModifiedOrderFiltersEvent e)
        {
            SendPlayerModifiedOrderFilters(Client.Instance, e.Order, e.Bone, e.Refinement, e.Material);
        }
        static void SendPlayerModifiedOrderFilters(NetEndpoint net, OrderSettings order, BoneDef bone, MaterialRefinementDef form, MaterialDef material)
        {
            net.BeginPacket(_pPlayerModifiedOrderFilters)
                .Write(order.Id)
                .Write(bone)
                .Write(form)
                .Write(material?.Name ?? "");
        }
        private static void OnPlayerModifiedOrderFilters(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var order = endpoint.Map.Town.CraftingManagerNew.GetOrder(r.ReadInt32());
            var bone = r.ReadDef<BoneDef>();
            var refinement = r.ReadDef<MaterialRefinementDef>();  //r.ReadString() is string refName && !refName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<MaterialRefinementDef>(refName) : null;
            var material = r.ReadString() is string matName && !matName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<MaterialDef>(matName) : null;
            order.Toggle(bone, refinement, material);
            if (endpoint is Server)
                SendPlayerModifiedOrderFilters(endpoint, order, bone, refinement, material);
        }

        

        private static void HandleCraftOrderCompletedEvent(CraftOrderCompletedEvent e)
        {
            e.Order.Workstation.Map.Net.BeginPacket(_pOrderUpdated)
                .Write(e.Order.Id)
                .Write(e.Actor.RefId);
        }
        private static void OnCraftOrderUpdated(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var order = endpoint.Map.Town.CraftingManagerNew.GetOrder(r.ReadInt32());
            var actor = endpoint.World.GetEntity<Actor>(r.ReadInt32());
            order.CompletedBy(actor);
        }

        private static void HandlePlayerIssuedCraftOrderEvent(PlayerIssuedCraftOrderEvent e)
        {
            var workstation = e.Workstation;
            var net = workstation.Map.World.Net;
            if(net is Client)
                SendPlayerCreatedOrderNew(e.Workstation.Parent, e.Craftable);
        }
        internal static void SendPlayerCreatedOrderNew(BlockEntity workstation, Def recipeDef)
        {
            var net = workstation.Map.Net;
            var w = net.BeginPacketImmediate(_pPlayerCreatedOrder)
                .Write(workstation.Map.ID)
                .Write(workstation.OriginGlobal)
                .Write(recipeDef);
        }
        internal static void PlayerCreatedOrder(BlockEntity workstation, MaterialRefinementDef processDef)
        {
            var net = workstation.Map.Net;
            var w = net.BeginPacket(_pPlayerCreatedOrder);
            w.Write(workstation.Map.ID);
            w.Write(workstation.OriginGlobal);
            w.Write(processDef);
        }
        private static void OnPlayerCreatedOrder(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var mapid = r.ReadInt32();
            var workstationPosition = r.ReadIntVec3();
            var refinement = r.ReadDef();
            if(net.Map.Town.CraftingManagerNew.CreateOrderNew(workstationPosition, refinement) is OrderSettings order &&
                net is Server server)
                SendPlayerCreatedOrderNew(net.Map.GetBlockEntity(workstationPosition), refinement);

            return;
            var station = r.ReadVector3();
            var reaction = r.ReadDef<Reaction>();
            net.Map.Town.CraftingManager.AddOrder(station, reaction);
            if (net is Server)
                Send(net, station, reaction);
        }
        internal static void SendPlayerDeletedOrder(MapBase map, OrderSettings order)
        {
            var net = map.Net;
            var w = net.BeginPacketImmediate(_pPlayerDeletedOrder);
            w.Write(map.ID);
            w.Write(order.Id);
        }
        private static void OnPlayerDeletedOrder(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var mapid = r.ReadInt32();
            var map = net.World.GetMap(mapid);
            var order = net.Map.Town.CraftingManagerNew.DeleteOrder(r.ReadInt32());
            if (net is Server server)
                SendPlayerDeletedOrder(map, order);
        }
        internal static void SendPlayerModifiedOrder(MapBase map, OrderSettings order, int priorityDelta, int amountDelta, OrderSettings.CraftMode mode)
        {
            var net = map.Net;
            var w = net.BeginPacketImmediate(_pPlayerModifiedOrder);
            w.Write(map.ID);
            w.Write(order.Id);
            w.Write(priorityDelta);
            w.Write(amountDelta);
            w.Write((int)mode);
        }
        private static void OnPlayerModifiedOrder(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var map = endpoint.World.GetMap(mapid);
            var order = endpoint.Map.Town.CraftingManagerNew.GetOrder(r.ReadInt32());
            var priorityDelta = r.ReadInt32();
            var amountDelta = r.ReadInt32();
            var mode = (OrderSettings.CraftMode)r.ReadInt32();
            order.Amount += amountDelta;

            // todo reorder based on priority delta
            order.ChangePriority(priorityDelta);
            order.Mode = mode;
            map.Events.Post(new CraftOrderUpdatedEvent(order));
            if (endpoint is Server server)
                SendPlayerModifiedOrder(map, order, priorityDelta, amountDelta, mode);
        }
        internal static void Send(NetEndpoint net, Vector3 global, Reaction reaction)
        {
            var w = net.BeginPacketImmediate(_pPlayerCreatedOrder);
            w.Write(global);
            reaction.Write(w);
        }
    }
}
