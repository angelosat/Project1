using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework;
using System.Data;
using System.Runtime.InteropServices;

#nullable enable

namespace Project1.Core.World.Actions;

[EnsureStaticCtorCall]
static class Packets_WorldActions
{
    static readonly PacketId
        _pActionStart = Registry.PacketHandlers.Register(ReceiveWorldActionStart),
        _pActionFinish = Registry.PacketHandlers.Register(ReceiveWorldActionFinish);

    static Packets_WorldActions()
    {
        Registry.WorldEventHooksServer.Register<WorldActionStartedEvent>(SendWorldActionStarted);
        Registry.WorldEventHooksServer.Register<WorldActionFinishedEvent>(SendWorldActionFinished);
    }
    private static void SendWorldActionStarted(WorldActionStartedEvent e)
    {
        Server.Instance.BeginPacket(_pActionStart)
            .Write(e.Actor.RefId)
            .Write(e.Action.Data.Frontier.Def)
            .Write(e.Action.Def)
            .Write(e.Action.Max)
            .Write(e.Action.Data);
            //.Write(e.Action.Data.Frontier.Def);
    }

    private static void SendWorldActionFinished(WorldActionFinishedEvent e)
    {
        Server.Instance.BeginPacket(_pActionFinish)
            .Write(e.Actor.RefId);
    }

    private static void ReceiveWorldActionStart(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var world = (StaticWorld)endpoint.World;
        var actor = world.Get<Actor>(r.ReadEntityRefId());
        var frontierDef = r.ReadDef<FrontierDef>();
        var actionDef = r.ReadDef<WorldActionDef>();
        var actionDuration = r.ReadInt32();
        var frontier = ((FrontierManager)world.Space).Get(frontierDef);
        //var runtime = world.Actions.Start(actionDef, actor!, frontierDef);
        var data = WorldActionSystem.CreateData(actionDef, frontier, actor);
        data.Read(r);
        var runtime = world.Actions.Begin(actionDef, data);
    }

    private static void ReceiveWorldActionFinish(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var world = (StaticWorld)endpoint.World;
        var actor = world.Get<Actor>(r.ReadEntityRefId());
        world.Actions.Finish(actor);
    }
}
