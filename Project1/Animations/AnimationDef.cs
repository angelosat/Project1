using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public sealed class AnimationDef : Def
    {
        public Dictionary<BoneDef, AnimationClip> KeyFrames = new Dictionary<BoneDef, AnimationClip>();
        public Dictionary<float, Action<GameObject>> Events = new Dictionary<float, Action<GameObject>>();
        public int Layer;
        public float Speed = 1;
        public int FrameCount;
        public WarpMode WarpMode;
        public Func<GameObject, float> WeightChangeFunc;

        AnimationDef(string name, int layer):base(name)
        {
            this.Layer = layer;
        }
        AnimationDef AddClip(BoneDef bone, AnimationClip clip)
        {
            this.KeyFrames[bone] = clip;
            this.FrameCount = Math.Max(this.FrameCount, clip.FrameCount);
            this.WarpMode = clip.WarpMode;
            return this;
        }
        AnimationDef AddClip(BoneDef bone, WarpMode mode, params Keyframe[] frames)
        {
            var clip = new AnimationClip(mode, frames);
            this.WarpMode = clip.WarpMode;
            this.KeyFrames[bone] = clip;
            this.FrameCount = Math.Max(this.FrameCount, clip.FrameCount);
            return this;
        }
        AnimationDef AddClip(BoneDef bone, params Keyframe[] frames)
        {
            var clip = new AnimationClip(WarpMode.Loop, frames);
            this.WarpMode = clip.WarpMode;
            this.KeyFrames[bone] = clip;
            this.FrameCount = Math.Max(this.FrameCount, clip.FrameCount);
            return this;
        }
        AnimationDef AddEvent(float frame, Action<GameObject> action)
        {
            this.Events[frame] = action;
            return this;
        }
        // TODO: load externally

        static public readonly AnimationDef Null = new AnimationDef("AnimationNull", 0);

        static public readonly AnimationDef Tool = new AnimationDef("AnimationTool", 2)
            .AddClip(BoneDefOf.RightHand, WarpMode.Loop,
                            new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
                            new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),
                            new Keyframe(20, Vector2.Zero, -(float)Math.PI / 2f),
                            new Keyframe(60, Vector2.Zero, -4 * (float)Math.PI / 3f, Interpolation.Sine)
                            )
                .AddClip(BoneDefOf.Torso,
                    new AnimationClip(WarpMode.Loop,
                        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f),
                        new Keyframe(60, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Exp)
                        ))
                .AddClip(BoneDefOf.Mainhand,
                    new AnimationClip(WarpMode.Loop,
                        new Keyframe(0, Vector2.Zero, 0, Interpolation.Exp),
                        new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                        new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f),
                        new Keyframe(60, Vector2.Zero, 0, Interpolation.Exp)
                ))
            .AddEvent(10, e => (e as Actor).Work.OnToolContact());

        static public readonly AnimationDef Work = new AnimationDef("AnimationWork", 2)
            .AddClip(BoneDefOf.RightHand, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
                new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp))
            .AddClip(BoneDefOf.Hips, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(15, new Vector2(0, -8), 0, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine))
            .AddClip(BoneDefOf.RightFoot, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(15, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftFoot, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine));

        static public readonly AnimationDef Walk = new AnimationDef("AnimationWalk", 1)
            .AddClip(BoneDefOf.Hips, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
                new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine))
            .AddEvent(20, e => Components.MobileComponent.OnFootDown(e))
            .AddClip(BoneDefOf.RightHand, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftHand, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine))
            .AddClip(BoneDefOf.RightFoot, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftFoot, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine))
            .AddClip(BoneDefOf.Head, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(5, new Vector2(0, 2), 0, Interpolation.Sine),
                new Keyframe(10, new Vector2(0, 0), 0, Interpolation.Sine),
                new Keyframe(15, new Vector2(0, -2), 0, Interpolation.Sine),
                new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));

        static public readonly AnimationDef Jump = new AnimationDef("AnimationJump", 2) { Speed = 0 }
            .AddClip(BoneDefOf.RightHand, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftHand, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.RightFoot, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftFoot, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.Torso, new Keyframe(0, Vector2.Zero, 0))
            .AddClip(BoneDefOf.Hips, new Keyframe(0, Vector2.Zero, 0))
            .AddClip(BoneDefOf.Head, new Keyframe(0, Vector2.Zero, 0));

        static public readonly AnimationDef Crouch = new AnimationDef("AnimationCrouch", layer: 4) { Speed = 0 } //  layer: 2
            .AddClip(BoneDefOf.Torso, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f));

        static public readonly AnimationDef Haul = new AnimationDef("AnimationHaul", 3) { WeightChangeFunc = actor=>actor.Hauled != null ? .1f : -1f }
            .AddClip(BoneDefOf.RightHand, new AnimationClip(WarpMode.Once,
                new Keyframe(0, Vector2.Zero, -(float)Math.PI)
                ))
            .AddClip(BoneDefOf.LeftHand, new AnimationClip(WarpMode.Once,
                new Keyframe(0, Vector2.Zero, -(float)Math.PI)
                ))
            .AddClip(BoneDefOf.Torso, new AnimationClip(WarpMode.Once,
                new Keyframe(0, Vector2.Zero, 0)))
            ;

        static public readonly AnimationDef TouchItem = new AnimationDef("AnimationTouchItem", 4)
            .AddClip(BoneDefOf.RightHand, WarpMode.Once,
                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine)
                )

            .AddClip(BoneDefOf.LeftHand, new AnimationClip(WarpMode.Once,
                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine)
                ))
            .AddClip(BoneDefOf.Torso, new AnimationClip(WarpMode.Once,
                new Keyframe(0, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine)
                ))
            .AddEvent(25, e => (e as Actor).Work.OnToolContact());

        static public readonly AnimationDef DeliverAttack = new AnimationDef("AnimationDeliverAttack", 4)
            .AddClip(BoneDefOf.RightHand, WarpMode.Once,
                new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
                new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),
                new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp))
            .AddClip(BoneDefOf.Mainhand, WarpMode.Once,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp))
            .AddClip(BoneDefOf.Torso, WarpMode.Clamp,
                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f),
                new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f),
                new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f));

        static AnimationDef()
        {
            Register(Null);
            Register(TouchItem);
            Register(Walk);
            Register(Jump);
            Register(Crouch);
            Register(Tool);
            Register(Haul);
            Register(Work);
            Register(DeliverAttack);
        }
    }
}
