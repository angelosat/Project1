using Project1.Core.Entities.Actors;
using Project1.Core.World.WorldAreas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project1.Core.World.Actions;

internal abstract class WorldActionOutcome
{
    internal abstract void Apply(FrontierRuntime frontier, Actor actor);
}

internal sealed class WorldActionOutcome_Loot : WorldActionOutcome
{
    internal override void Apply(FrontierRuntime frontier, Actor actor)
    {
        throw new NotImplementedException();
    }
}
