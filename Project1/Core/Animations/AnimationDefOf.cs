using System;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Core.Entities.Actors;

namespace Project1.Core.Animations
{
    [EnsureStaticCtorCall]
    static class AnimationDefOf
    {

        static public readonly AnimationDef Null = new AnimationDef("AnimationNull", 0);
        static public readonly AnimationDef Tool = new AnimationDef("AnimationTool", 2)
            .AddClip(BoneDefOf.RightHand, WarpMode.Loop,
                            new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f),
                            new Keyframe(40, Vector2.Zero, -4 * (float)Math.PI / 3f, Interpolation.Sine),
                            new Keyframe(50, Vector2.Zero, -4 * (float)Math.PI / 3f),
                            new Keyframe(60, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),
                            new Keyframe(70, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                            )
                .AddClip(BoneDefOf.Torso,
                    new AnimationClip(WarpMode.Loop,
                        new Keyframe(00, Vector2.Zero, (float)Math.PI / 8f),
                        new Keyframe(40, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(50, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(60, Vector2.Zero, (float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(70, Vector2.Zero, (float)Math.PI / 8f, Interpolation.Exp)

                        ))
                .AddClip(BoneDefOf.Mainhand,
                    new AnimationClip(WarpMode.Loop,
                        new Keyframe(00, Vector2.Zero, (float)Math.PI / 2f),
                        new Keyframe(40, Vector2.Zero, 0, Interpolation.Exp),
                        new Keyframe(50, Vector2.Zero, 0, Interpolation.Exp),
                        new Keyframe(60, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                        new Keyframe(70, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp)

                ))
            .AddEvent(60, e => (e as Actor).Work.OnToolContact());
        
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
        //{ 
        //    WeightGetter = a => a.Mobile.Speed,
        //    SpeedGetter = a => a.Mobile.Speed
        //}
            .AddClip(BoneDefOf.Hips, WarpMode.Loop,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
                new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine))
            .AddEvent(20, MobileComponent.OnFootstep)
            .AddEvent(40, MobileComponent.OnFootstep)
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

        static public readonly AnimationDef Jump = new AnimationDef("AnimationJump", 2) { Speed = 0, WeightGetter = e => e.Physics.MidAir ? 1 : 0 }
            .AddClip(BoneDefOf.RightHand, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftHand, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.RightFoot, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.LeftFoot, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine))
            .AddClip(BoneDefOf.Torso, new Keyframe(0, Vector2.Zero, 0))
            .AddClip(BoneDefOf.Hips, new Keyframe(0, Vector2.Zero, 0))
            .AddClip(BoneDefOf.Head, new Keyframe(0, Vector2.Zero, 0));

        static public readonly AnimationDef Crouch = new AnimationDef("AnimationCrouch", layer: 4) { Speed = 0 } //  layer: 2
            .AddClip(BoneDefOf.Torso, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f));

        static public readonly AnimationDef Haul = new AnimationDef("AnimationHaul", 3) { WeightChangeFunc = actor => actor.Hauled != null ? .1f : -1f }
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

        static AnimationDefOf()
        {
            Def.Register(typeof(AnimationDefOf));
            //Register(Null);
            //Register(TouchItem);
            //Register(Walk);
            //Register(Jump);
            //Register(Crouch);
            //Register(Tool);
            //Register(Haul);
            //Register(Work);
            //Register(DeliverAttack);
        }
    }
}
