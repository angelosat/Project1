using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_;

namespace Project1.Framework.Animations
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
        public Func<Entity, float> WeightGetter;

        public AnimationDef(string name, int layer):base(name)
        {
            this.Layer = layer;
        }
        public AnimationDef AddClip(BoneDef bone, AnimationClip clip)
        {
            this.KeyFrames[bone] = clip;
            this.FrameCount = Math.Max(this.FrameCount, clip.FrameCount);
            this.WarpMode = clip.WarpMode;
            return this;
        }
        public AnimationDef AddClip(BoneDef bone, WarpMode mode, params Keyframe[] frames)
        {
            var clip = new AnimationClip(mode, frames);
            this.WarpMode = clip.WarpMode;
            this.KeyFrames[bone] = clip;
            this.FrameCount = Math.Max(this.FrameCount, clip.FrameCount);
            return this;
        }
        public AnimationDef AddClip(BoneDef bone, params Keyframe[] frames)
        {
            var clip = new AnimationClip(WarpMode.Loop, frames);
            this.WarpMode = clip.WarpMode;
            this.KeyFrames[bone] = clip;
            this.FrameCount = Math.Max(this.FrameCount, clip.FrameCount);
            return this;
        }
        public AnimationDef AddEvent(float frame, Action<GameObject> action)
        {
            this.Events[frame] = action;
            return this;
        }
        // TODO: load externally

    }
}
