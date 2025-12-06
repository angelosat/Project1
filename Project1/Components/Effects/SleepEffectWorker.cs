using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;
using System;

namespace Start_a_Town_
{
    internal class SleepEffectWorker : EntityEffectWorker
    {
        public override void OnStart(Actor actor)
        {
            actor.GetNeed(NeedDef.Energy).AddMod(EffectDefOf.Sleeping, 0, 1);
        }
        public override void OnFinish(Actor actor)
        {
            actor.GetNeed(NeedDef.Energy).RemoveMod(EffectDefOf.Sleeping);
        }
    }
}
