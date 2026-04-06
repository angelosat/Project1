using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Effects;
using Project1.Core.Needs;
using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Interactions
{
    sealed class InteractionSleepInBedLogic : InteractionLogic
    {
        sealed class Context : InteractionContext
        {
            NeedRuntime _energy;
            NeedRuntime Energy => _energy ??= this.Actor.GetNeed(NeedDefOf.Energy);
            internal EntityEffectWrapper Effect => field ??= this.Actor.Effects.GetEffect(EffectDefOf.ModifyNeed, NeedDefOf.Energy);
            public override float ProgressBarPercentage => this.Energy.Percentage;
        }
        protected override InteractionContext CreateContextInt() => new Context();
        public override bool CanPerform(InteractionContext ctx)
        {
            var typedCtx = (Context)ctx;
            if (typedCtx.Effect.IsFinished)
                return false;
            return true;
        }
        internal override void OnStart(Interaction i)
        {
            var a = i.Actor;
            if (a.Net.IsClient)
                return;
            var t = i.Target;
            var map = a.Map;
            var bedPos = t.Global; // the bed position passed should be the origin cell
            a.SetPosition(bedPos + new Vector3(0, 0, Block.GetBlockHeight(a.Map, bedPos)));
            //a.Net.LogStateChange(a);
            //a.Effects.Apply(EffectDefOf.Sleeping);
            a.Effects.Apply(new EntityEffectWrapper(EffectDefOf.ModifyNeed, NeedDefOf.Energy, null, Ticks.FromMinutes(1))); //1));
            //a.Effects.Apply(new EntityEffectWrapper(EffectDefOf.ModifyNeed, NeedDefOf.Energy, null, 1));

            var topcell = map.GetCell(bedPos);
            var testcell = map.GetCell((IntVec3)bedPos + IntVec3.UnitY);
            var bedparts = topcell.GetParts(bedPos).ToArray();
            var bedFeet = bedparts[1];
            a.FaceTowards(bedFeet);

            //var body = a.Body;
            //var headBone = a.Body.FindBone(BoneDefOf.Head);
            //var headOffset = headBone.GetTotalOffset();
            //body.RestingFrame = new Keyframe(0, headOffset, 0);

            //body.SetEnabled(false, true);
            //headBone.SetEnabled(true, false);
            ////headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
            //headBone.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f));
            var comp = a.SpriteComp;
            var body = a.Body;
            var headBone = a.Body.FindBone(BoneDefOf.Head);
            var headOffset = headBone.GetTotalOffset();
            comp.OverrideRestingFrame(body.Def, new Keyframe(0, headOffset, 0));
            comp.ToggleBone(body.Def, false, true);
            comp.ToggleBone(headBone.Def, true, false);
            comp.OverrideRestingFrame(headBone.Def, new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f)));


            //var bed = map.GetBlockEntity<BlockBedEntity>(t.Global);
            //bed.Owner = a;

            var room = map.Town.RoomManager.GetRoomAt(t.Global);
            if (room is not null)
            {
                if (room.Owner is null)
                    a.Possessions.Claim(room);
                else if (room.Owner != a || room.Workplace != null)
                    throw new Exception();
            }
        }
        internal override void OnFinish(Interaction i)
        {
            var a = i.Actor;
            if (a.Net.IsClient)
                return;
            var t = i.Target;
            //a.Effects.Remove(EffectDefOf.Sleeping);
            a.Effects.Abort(EffectDefOf.ModifyNeed, NeedDefOf.Energy);

            //var body = a.Body;
            //var head = body.FindBone(BoneDefOf.Head);

            //body.SetEnabled(true, true);
            //body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            //head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);


            var body = a.Body;
            var head = body.FindBone(BoneDefOf.Head);
            var comp = a.SpriteComp;

            comp.ToggleBone(body.Def, true, true);
            comp.OverrideRestingFrame(body.Def, new Keyframe(0, Vector2.Zero, 0));
            comp.OverrideRestingFrame(head.Def, new Keyframe(0, Vector2.Zero, 0));

            var interactionSpots = Cell.GetFreeInteractionSpots(a.Map, t.Global, a);
            if (interactionSpots.Any())
                a.SetPosition(interactionSpots.First());
        }
    }
}
