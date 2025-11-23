using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;
using System.Linq;

namespace Start_a_Town_.Blocks.Bed
{
    class InteractionSleepInBed : Interaction
    {
        public InteractionSleepInBed()
            : base("Sleeping in bed")
        {
            this.RunningType = RunningTypes.Continuous;
            this.Animation = null;
        }
        internal override void InitAction()
        {
            var a = this.Actor;
            var t = this.Target;
            var map = a.Map;
            var bedPos = t.Global; // the bed position passed should be the origin cell
            a.SetPosition(bedPos + new Vector3(0, 0, Block.GetBlockHeight(a.Map, bedPos)));
            a.GetNeed(NeedDef.Energy).AddMod(NeedLetDefOf.Sleeping, 0, 1);
            a.GetNeed(NeedDef.Comfort).AddMod(NeedLetDefOf.Sleeping, 20, 0);

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
            a.GetNeed(NeedDef.Energy).RemoveMod(NeedLetDefOf.Sleeping);
            a.GetNeed(NeedDef.Comfort).RemoveMod(NeedLetDefOf.Sleeping);

            var spriteComp = a.GetComponent<SpriteComponent>();
            var body = a.Body;
            var head = body.FindBone(BoneDefOf.Head);

            body.SetEnabled(true, true);
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            var interactionSpots = Cell.GetFreeInteractionSpots(a.Map, t.Global, a);
            if(interactionSpots.Any())
                a.SetPosition(interactionSpots.First());
        }
        public override object Clone()
        {
            return new InteractionSleepInBed();
        }
    }
}
