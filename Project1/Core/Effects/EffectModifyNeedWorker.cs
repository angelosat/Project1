using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using System;

namespace Project1.Core.Effects
{
    internal class EffectModifyNeedWorker : EntityEffectWorker
    {
        protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
        {
            //var comp = actor.SpriteComp;
            //var body = actor.Body;
            //var headBone = actor.Body.FindBone(BoneDefOf.Head);
            //var headOffset = headBone.GetTotalOffset();
            //comp.OverrideRestingFrame(body.Def, new Keyframe(0, headOffset, 0));
            //comp.ToggleBone(body.Def, false, true);
            //comp.ToggleBone(headBone.Def, true, false);
            //comp.OverrideRestingFrame(headBone.Def, new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f)));
        }
        protected override void OnTick(Actor actor, EntityEffectWrapper wrapper)
        {
            var need = actor.Needs.NeedsNew[(NeedDef)wrapper.Target];
            need.Accumulator += 1f / wrapper.TicksPerUnit;
        }
        protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
        {
            //var body = actor.Body;
            //var head = body.FindBone(BoneDefOf.Head);
            //var comp = actor.SpriteComp;

            //comp.ToggleBone(body.Def, true, true);
            //comp.OverrideRestingFrame(body.Def, new Keyframe(0, Vector2.Zero, 0));
            //comp.OverrideRestingFrame(head.Def, new Keyframe(0, Vector2.Zero, 0));
        }
    }
    //internal class EffectModifyNeedWorker : EntityEffectWorker
    //{
    //    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    //    {
    //        actor.SpriteComp.ToggleBone(actor.Body.Def, false, true);
    //        var comp = actor.SpriteComp;
    //        var body = actor.Body;
    //        var headBone = actor.Body.FindBone(BoneDefOf.Head);
    //        var headOffset = headBone.GetTotalOffset();
    //        //body.RestingFrame = new Keyframe(0, headOffset, 0);
    //        //body.SetEnabled(false, true);
    //        //headBone.SetEnabled(true, false);
    //        //headBone.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f));
    //        comp.OverrideRestingFrame(body.Def, new Keyframe(0, headOffset, 0));
    //        comp.ToggleBone(body.Def, false, true);
    //        comp.ToggleBone(headBone.Def, true, false);
    //        comp.OverrideRestingFrame(headBone.Def, new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f)));
    //    }
    //    protected override void OnTick(Actor actor, EntityEffectWrapper wrapper)
    //    {
    //        var need = actor.Needs.NeedsNew[(NeedDef)wrapper.Target];
    //        need.Accumulator += 1f / wrapper.TicksPerUnit;
    //    }
    //    protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
    //    {
    //        var body = actor.Body;
    //        var head = body.FindBone(BoneDefOf.Head);
    //        var comp = actor.SpriteComp;

    //        //body.SetEnabled(true, true);
    //        //body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
    //        //head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);

    //        comp.ToggleBone(body.Def, true, true);
    //        comp.OverrideRestingFrame(body.Def, new Keyframe(0, Vector2.Zero, 0));
    //        comp.OverrideRestingFrame(head.Def, new Keyframe(0, Vector2.Zero, 0));
    //    }
    //}
    //internal class EffectModifyNeedWorker : EntityEffectWorker
    //{
    //    protected override void OnStart(Actor actor, EntityEffectWrapper wrapper)
    //    {
    //        var body = actor.Body;
    //        var headBone = actor.Body.FindBone(BoneDefOf.Head);
    //        var headOffset = headBone.GetTotalOffset();
    //        body.RestingFrame = new Keyframe(0, headOffset, 0);

    //        body.SetEnabled(false, true);
    //        headBone.SetEnabled(true, false);
    //        headBone.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f));


    //        //var need = actor.GetNeed((NeedDef)wrapper.Target);
    //        //if (wrapper.IsInstant)
    //        //    need.ApplyDelta(wrapper.Budget.Value);
    //        //else
    //        //    need.AddMod(EffectDefOf.ModifyNeed, wrapper.Rate);
    //    }
    //    protected override void OnTick(Actor actor, EntityEffectWrapper wrapper)
    //    {
    //        //if (wrapper.RemainingBudget == 0)
    //        //    wrapper.IsFinished = true;
    //        //actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
    //        var need = actor.Needs.NeedsNew[(NeedDef)wrapper.Target];
    //        need.Accumulator += 1f / wrapper.TicksPerUnit;
    //    }
    //    protected override void OnFinish(Actor actor, EntityEffectWrapper wrapper)
    //    {
    //        var body = actor.Body;
    //        var head = body.FindBone(BoneDefOf.Head);
    //        body.SetEnabled(true, true);
    //        body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
    //        head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
    //        //if (!wrapper.IsInstant)
    //        //actor.GetNeed((NeedDef)wrapper.Target).RemoveMod(EffectDefOf.ModifyNeed);
    //    }
    //}
}
