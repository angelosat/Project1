using Project1.Core.Systems.Thoughts;
using System;

namespace Project1.Core.World.Encounters
{
    sealed class Thought_Injury : ThoughtSource<ActorOffmapHealthEvent>
    {
        internal override void Handle(ActorOffmapHealthEvent e)
        {
            var heal = e.Delta > 0;
            e.Actor.AI.State.Log.Write($"{(heal ? "Recovered" : "Lost")} {Math.Abs(e.Delta)} health");
        }
    }
}
