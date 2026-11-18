using Project1.Core.Entities.Actors;
using Project1.Framework.Helpers;

namespace Project1.Core.World.Actions;

internal static class WorldActionSystem
{
    internal static WorldActionData CreateData(WorldActionDef def, Actor actor)
    => ActivatorSafe<WorldActionData>.CreateInstance(def.DataType, actor);
    //{
    //    //if(def.DataType is not null)
    //    //    return ActivatorSafe<WorldActionData>.CreateInstance(def.DataType, frontier, actor);
    //    return def.Worker.CreateData(frontier, actor);
    //}
}
