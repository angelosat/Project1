using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Core.Towns.Stockpiles;
using Project1.Framework;
using System;

namespace Project1.Core.Systems.Crafting;

[EnsureStaticCtorCall]
static class Packets_Crafting
{
    readonly static PacketId 
        _pPlayerCancellingUnfinished,
        _pPlayerCreatedOrderNew,
        _pPlayerDeletedOrder, 
        _pPlayerModifiedOrder, 
        _pOrderUpdated, 
        _pPlayerModifiedOrderFilters, 
        _pPlayerSetWorkstationIO,
        _pActorFinishedCrafting = Registry.PacketHandlers.Register(ReceiveActorFinishedCrafting);

 

    static Packets_Crafting()
    {
        _pPlayerSetWorkstationIO = Registry.PacketHandlers.Register(OnPlayerSetWorkstationIO);

        _pPlayerCreatedOrderNew = Registry.PacketHandlers.Register(OnPlayerCreatedOrderNew);
        _pPlayerDeletedOrder = Registry.PacketHandlers.Register(OnPlayerDeletedOrder);
        _pPlayerModifiedOrder = Registry.PacketHandlers.Register(OnPlayerModifiedOrder);
        _pPlayerModifiedOrderFilters = Registry.PacketHandlers.Register(OnPlayerModifiedOrderFilters);
        _pOrderUpdated = Registry.PacketHandlers.Register(OnCraftOrderUpdated);
        _pPlayerCancellingUnfinished = Registry.PacketHandlers.Register(OnPlayerCancellingUnfinished);
        Registry.PlayerInputEventHooks.Register<PlayerIssuedCraftOrderEventNew>(HandlePlayerIssuedCraftOrderNew);
        Registry.PlayerInputEventHooks.Register<PlayerModifiedOrderFiltersEvent>(HandlePlayerModifiedOrderFilters);
        Registry.PlayerInputEventHooks.Register<PlayerSetWorkstationZoneEvent>(HandlePlayerSetWorkstationZoneEvent);
        Registry.MapEventHooksServer.Register<CraftOrderCompletedEvent>(HandleCraftOrderCompletedEvent);
        Registry.PlayerInputEventHooks.Register<PlayerCancellingUnfinishedItemEvent>(HandlePlayerCancellingUnfinishedItem);
        Registry.WorldEventHooksServer.Register<ActorFinishedCraftingEvent>(HandleActorFinishedCrafting);
    }

    private static void HandleActorFinishedCrafting(ActorFinishedCraftingEvent e)
    {
        Server.Instance.BeginPacket(_pActorFinishedCrafting).Write(e);
    }

    private static void ReceiveActorFinishedCrafting(NetEndpoint endpoint, Packet packet)
    {
        endpoint.World.Events.Post(ActorFinishedCraftingEvent.Create(packet.PacketReader));
    }

    private static void HandlePlayerCancellingUnfinishedItem(PlayerCancellingUnfinishedItemEvent e)
    {
        var item = e.Item;
        if(Ingame.Net is Server)
            ToolSystem.CancelUnfinished(item);
        else
            SendPlayerCancellingUnfinishedItem(Ingame.Net, item);
    }

    private static void SendPlayerCancellingUnfinishedItem(NetEndpoint net, Entity item)
    {
        net.BeginPacketImmediate(_pPlayerCancellingUnfinished)
            .Write(item.RefId);
    }

    private static void OnPlayerCancellingUnfinished(NetEndpoint endpoint, Packet packet)
    {
        if (endpoint.IsClient)
            throw new InvalidOperationException("Operation should never occur on a client");
        var r = packet.PacketReader;
        var item = endpoint.World.Get(r.ReadEntityRefId());
        ToolSystem.CancelUnfinished(item);
    }

    private static void HandlePlayerIssuedCraftOrderNew(PlayerIssuedCraftOrderEventNew e)
    {
        var workstation = e.Workstation;
        var net = workstation.Map.World.Net;
        if (net is Server)
            workstation.Map.Town.Crafting.CreateOrderNewInt(workstation.Parent.OriginGlobal, e.Request);
        SendPlayerCreatedOrderNew(e.Workstation.Parent, e.Request);
    }
    private static void SendPlayerCreatedOrderNew(BlockEntity workstation, AddOrderRequest req)
    {
        var net = workstation.Map.Net;
        var w = net.BeginPacketImmediate(_pPlayerCreatedOrderNew)
            .Write(workstation.Map.ID)
            .Write(workstation.OriginGlobal);
        req.Write(w);
    }
    private static void OnPlayerCreatedOrderNew(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var mapid = r.ReadMapId();
        var map = net.World.Get(mapid);
        var workstationPosition = r.ReadIntVec3();
        var req = AddOrderRequest.Create(r);
        if (map.Town.Crafting.CreateOrderNewInt(workstationPosition, req) is CraftingOrder order &&
            net is Server server)
            SendPlayerCreatedOrderNew(map.GetBlockEntity(workstationPosition), req);
    }
   

    private static void HandlePlayerSetWorkstationZoneEvent(PlayerSetWorkstationZoneEvent e)
    {
        SendPlayerSetWorkstationZone(Client.Instance, e.Workstation, e.IOType, e.Stockpile);
    }
    static void SendPlayerSetWorkstationZone(NetEndpoint net, BlockWorkstationComp comp, WorkstationIOType iotype, Stockpile stockpile)
    {
        net.BeginPacketImmediate(_pPlayerSetWorkstationIO)
            .Write(comp.Map.ID)
            .Write(comp.Parent.OriginGlobal)
            .Write((int)iotype)
            .Write(stockpile?.ID ?? 0);
    }
    private static void OnPlayerSetWorkstationIO(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var map = endpoint.World.Get((MapId)r.ReadInt32());
        var entity = map.GetBlockEntity(r.ReadIntVec3());
        var comp = entity.GetComp<BlockWorkstationComp>();
        var iotype = (WorkstationIOType)r.ReadInt32();
        var zoneid = (ZoneId)r.ReadInt32();
        var stockpile = map.Town.ZoneManager.GetZone<Stockpile>(zoneid);
        comp.SetStockpile(iotype, stockpile);
        if (endpoint is Server server)
            SendPlayerSetWorkstationZone(server, comp, iotype, stockpile);
    }
    private static void HandlePlayerModifiedOrderFilters(PlayerModifiedOrderFiltersEvent e)
    {
        SendPlayerModifiedOrderFilters(Client.Instance, e.Order, e.Bone, e.Refinement, e.Material);
    }
    static void SendPlayerModifiedOrderFilters(NetEndpoint net, CraftingOrder order, BoneDef bone, MaterialTypeDef form, MaterialDef material)
    {
        net.BeginPacket(_pPlayerModifiedOrderFilters)
            .Write(order.Workstation.Map.ID)
            .Write(order.Id)
            .Write(bone)
            .Write(form)
            .Write(material?.Name ?? "");
    }
    private static void OnPlayerModifiedOrderFilters(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var mapid = r.ReadMapId();
        var map = endpoint.World.Get(mapid);
        var order = map.Town.Crafting.Get(r.ReadInt32());
        var bone = r.ReadDef<BoneDef>();
        var refinement = r.ReadDef<MaterialTypeDef>();
        var material = r.ReadString() is string matName && !matName.IsNullEmptyOrWhiteSpace() ? Def.Get<MaterialDef>(matName) : null;
        order.Toggle(bone, refinement, material);
        if (endpoint is Server)
            SendPlayerModifiedOrderFilters(endpoint, order, bone, refinement, material);
    }

    private static void HandleCraftOrderCompletedEvent(CraftOrderCompletedEvent e)
    {
        e.Order.Workstation.Map.Net.BeginPacket(_pOrderUpdated)
            .Write(e.Order.Workstation.Map.ID)
            .Write(e.Order.Id)
            .Write(e.Actor.RefId);
    }
    private static void OnCraftOrderUpdated(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var mapid = r.ReadMapId();
        var map = endpoint.World.Get(mapid);
        var order = map.Town.Crafting.Get(r.ReadInt32());
        var actor = endpoint.World.Get<Actor>(r.ReadInt32());
        order.CompletedBy(actor);
    }

   
  
    internal static void SendPlayerDeletedOrder(MapBase map, CraftingOrder order)
    {
        var net = map.Net;
        var w = net.BeginPacketImmediate(_pPlayerDeletedOrder);
        w.Write(map.ID);
        w.Write(order.Id);
    }
    private static void OnPlayerDeletedOrder(NetEndpoint net, Packet pck)
    {
        var r = pck.PacketReader;
        var mapid = r.ReadMapId();
        var map = net.World.Get(mapid);
        var order = map.Town.Crafting.DeleteOrder(r.ReadInt32());
        if (net is Server server)
            SendPlayerDeletedOrder(map, order);
    }
    internal static void SendPlayerModifiedOrder(MapBase map, CraftingOrder order, int priorityDelta, int amountDelta, CraftingOrder.CraftMode mode)
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
        var mapid = r.ReadMapId();
        var map = endpoint.World.Get(mapid);
        var order = map.Town.Crafting.Get(r.ReadInt32());
        var priorityDelta = r.ReadInt32();
        var amountDelta = r.ReadInt32();
        var mode = (CraftingOrder.CraftMode)r.ReadInt32();
        order.Amount += amountDelta;

        // todo reorder based on priority delta
        order.ChangePriority(priorityDelta);
        order.Mode = mode;
        map.Events.Post(new CraftOrderUpdatedEvent(order));
        if (endpoint is Server server)
            SendPlayerModifiedOrder(map, order, priorityDelta, amountDelta, mode);
    }
}
