using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketPlayerCraftOrders
    {
        static int _pPlayerCreatedOrder, _pPlayerDeletedOrder, _pPlayerModifiedOrder;
        static PacketPlayerCraftOrders()
        {
            _pPlayerCreatedOrder = Registry.PacketHandlers.Register(OnPlayerCreatedOrder);
            _pPlayerDeletedOrder = Registry.PacketHandlers.Register(OnPlayerDeletedOrder);
            _pPlayerModifiedOrder = Registry.PacketHandlers.Register(OnPlayerModifiedOrder);
        }
        internal static void PlayerCreatedOrder(BlockEntity workstation, RawMaterialStateDef processDef)
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
            var workstationPosition = r.ReadIntVec3();// net.Map.GetBlockEntity();
            var refinement = r.ReadDef<RawMaterialStateDef>();
            net.Map.Town.CraftingManagerNew.CreateOrder(workstationPosition, refinement);
            if (net is Server server)
                PlayerCreatedOrder(net.Map.GetBlockEntity(workstationPosition), refinement);

            return;
            var station = r.ReadVector3();
            var reaction = r.ReadDef<Reaction>();
            net.Map.Town.CraftingManager.AddOrder(station, reaction);
            if (net is Server)
                Send(net, station, reaction);
        }
        internal static void PlayerDeletedOrder(MapBase map, OrderSettings order)
        {
            var net = map.Net;
            var w = net.BeginPacket(_pPlayerDeletedOrder);
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
                PlayerDeletedOrder(map, order);
        }
        internal static void PlayerModifiedOrder(MapBase map, OrderSettings order, int priorityDelta, int amountDelta, OrderSettings.CraftMode mode)
        {
            var net = map.Net;
            var w = net.BeginPacket(_pPlayerModifiedOrder);
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
            var order = endpoint.Map.Town.CraftingManagerNew.GetOrderBy(r.ReadInt32());
            var priorityDelta = r.ReadInt32();
            var amountDelta = r.ReadInt32();
            var mode = (OrderSettings.CraftMode)r.ReadInt32();
            order.Amount += amountDelta;

            // todo reorder based on priority delta
            order.ChangePriority(priorityDelta);

            order.Mode = mode;
            map.Events.Post(new CraftOrderModifiedEvent(order));
            if (endpoint is Server server)
                PlayerModifiedOrder(map, order, priorityDelta, amountDelta, mode);
        }
        internal static void Send(NetEndpoint net, Vector3 global, Reaction reaction)
        {
            var w = net.BeginPacket(_pPlayerCreatedOrder);
            w.Write(global);
            reaction.Write(w);
        }
        
        
    }
}
