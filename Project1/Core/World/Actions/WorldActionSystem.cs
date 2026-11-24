using Project1.Core.Entities.Actors;
using Project1.Framework.Helpers;

namespace Project1.Core.World.Actions;

internal static class WorldActionSystem
{
    internal static WorldActionData CreateData(WorldActionDef def, Actor actor)
        //=> ActivatorSafe<WorldActionData>.CreateInstance(def.DataType, actor);
        => ActivatorSafe<WorldActionData>.CreateInstance(def.Worker.DataType, actor);
}
