using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class InteractionSleepOnGround : Interaction
    {
        public InteractionSleepOnGround()
            : base("Sleeping on ground")
        {
            this.RunningType = RunningTypes.Continuous;
            this.Animation = null;
        }
        internal override void InitAction()
        {
            var a = this.Actor;
            var t = this.Target;
            //a.GetNeed(NeedDef.Energy).AddMod(EffectDefOf.Sleeping, 0, 1);
            //a.GetNeed(NeedDef.Comfort).AddMod(EffectDefOf.Sleeping, -20, 0);
            a.Effects.Apply(EffectDefOf.Sleeping);

            var body = a.Body;
            body.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 2f));
            body.OriginGroundOffset = new Vector2(0, -4);/// Vector2.Zero;
        }
        internal override void FinishAction()
        {
            var a = this.Actor;
            var t = this.Target; 
            //a.GetNeed(NeedDef.Energy).RemoveMod(EffectDefOf.Sleeping);
            //a.GetNeed(NeedDef.Comfort).RemoveMod(EffectDefOf.Sleeping);
            a.Effects.Remove(EffectDefOf.Sleeping);

            var body = a.Body;
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            //body.OriginGroundOffset = a.Def.Body.OriginGroundOffset;
            body.OriginGroundOffset = a.SpriteComp.Defaults.RootBone.OriginGroundOffset;
        }
        public override object Clone()
        {
            return new InteractionSleepOnGround();
        }
    }
}
