using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Core.Needs;
using Project1.Core.Effects;
using Project1.Core.Blocks;
using Project1.Core.Simulation;
using Project1.Core.Animations;

namespace Project1.Core.Interactions
{
    class InteractionSleepInBedLogic : InteractionLogic
    {
        class Context : InteractionContext
        {
            Need _energy;
            Need Energy => _energy ??= this.Actor.GetNeed(NeedDefOf.Energy);
            public override float ProgressPercentage => this.Energy.Percentage;
        }
        protected override InteractionContext CreateContextInternal() => new Context();

        internal override void OnStart(Interaction i)
        {
            var a = i.Actor;
            var t = i.Target;
            var map = a.Map;
            var bedPos = t.Global; // the bed position passed should be the origin cell
            a.SetPosition(bedPos + new Vector3(0, 0, Block.GetBlockHeight(a.Map, bedPos)));
            a.Effects.Apply(EffectDefOf.Sleeping);

            var topcell = map.GetCell(bedPos);
            var testcell = map.GetCell((IntVec3)bedPos + IntVec3.UnitY);
            var bedparts = topcell.GetParts(bedPos).ToArray();
            var bedFeet = bedparts[1];
            a.FaceTowards(bedFeet);

            var body = a.Body;
            var headBone = a.Body.FindBone(BoneDefOf.Head);
            var headOffset = headBone.GetTotalOffset();
            body.RestingFrame = new Keyframe(0, headOffset, 0);

            body.SetEnabled(false, true);
            headBone.SetEnabled(true, false);
            //headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
            headBone.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 3f));

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
            var t = i.Target;
            a.Effects.Remove(EffectDefOf.Sleeping);

            var spriteComp = a.GetComponent<SpriteComp>();
            var body = a.Body;
            var head = body.FindBone(BoneDefOf.Head);

            body.SetEnabled(true, true);
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            var interactionSpots = Cell.GetFreeInteractionSpots(a.Map, t.Global, a);
            if (interactionSpots.Any())
                a.SetPosition(interactionSpots.First());
        }
    }
    class InteractionSleepInBed : Interaction
    {
        public InteractionSleepInBed()
            : base("Sleeping in bed")
        {
            this.RunningType = RunningTypes.Continuous;
            //this.AnimationDef = null;
        }
        protected void OnStart()
        {
            var a = this.Actor;
            var t = this.Target;
            var map = a.Map;
            var bedPos = t.Global; // the bed position passed should be the origin cell
            a.SetPosition(bedPos + new Vector3(0, 0, Block.GetBlockHeight(a.Map, bedPos)));
            a.Effects.Apply(EffectDefOf.Sleeping);

            var bedFeet = map.GetCell(bedPos).GetParts(bedPos).Skip(1).First();
            a.FaceTowards(bedFeet);

            var body = a.Body;
            var headBone = a.Body.FindBone(BoneDefOf.Head);
            var headOffset = headBone.GetTotalOffset();
            body.RestingFrame = new Keyframe(0, headOffset, 0);

            body.SetEnabled(false, true);
            headBone.SetEnabled(true, false);
            headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));

            var bed = map.GetBlockEntity<BlockBedEntity>(t.Global);
            bed.Owner = a;

            var room = map.Town.RoomManager.GetRoomAt(t.Global);
            if (room is not null)
            {
                if (room.Owner is null)
                    a.Possessions.Claim(room);
                else if (room.Owner != a || room.Workplace != null)
                    throw new Exception();
            }
        }
        internal override void FinishAction()
        {
            var a = this.Actor;
            var t = this.Target;
            a.Effects.Remove(EffectDefOf.Sleeping);

            var spriteComp = a.GetComponent<SpriteComp>();
            var body = a.Body;
            var head = body.FindBone(BoneDefOf.Head);

            body.SetEnabled(true, true);
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            var interactionSpots = Cell.GetFreeInteractionSpots(a.Map, t.Global, a);
            if(interactionSpots.Any())
                a.SetPosition(interactionSpots.First());
        }
    }
}
