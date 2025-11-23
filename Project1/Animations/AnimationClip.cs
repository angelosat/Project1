using System;
using System.Collections.Generic;
using Start_a_Town_.Components.Animations;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public enum WarpMode { Once, Loop, PingPong, Clamp }
    public enum AnimationStates { Running, Finished, Removed }
    public class AnimationClip
    {
        public List<Keyframe> Keyframes = new List<Keyframe>();
        public Func<float, float, float, float> FadeInterpolation = Interpolation.Lerp;
        public bool PreFade;
        public int Duration; // if duration is negative, the state will remain running instead of finished when it's through
        public int FrameCount;
        public string Name;
        public AnimationBlending Blending;
        public AnimationStates State;
        public WarpMode WarpMode;
     
        public Dictionary<float, Action<GameObject>> Actions = new();

        public AnimationClip(WarpMode loop, params Keyframe[] kfs)
            : base()
        {
            this.State = AnimationStates.Running;
            this.Blending = AnimationBlending.Override;
            this.WarpMode = loop;
            foreach (Keyframe kf in kfs)
                this.AddFrame(kf);
            this.Duration = FrameCount;
        }

        public AnimationClip AddEvent(float frame, Action<GameObject> action)
        {
            this.Actions.Add(frame, action);
            return this;
        }

        public void AddFrame(Keyframe kf)
        {
            this.Add(kf);
            FrameCount = Math.Max(FrameCount, kf.Time);
        }

        public void Add(Keyframe kf)
        {
            this.Keyframes.Add(kf);
        }
        public Keyframe this[int id]
        {
            get { return this.Keyframes[id]; }
        }

        public void GetValue(float frame, float fade, out Vector2 offset, out float angle)
        {
            // if fading in, return a percentage of the first frame
            //var fade = ani.Fade;
            if (fade < 1)
            {
                Keyframe first = this[0];
                var t = this.FadeInterpolation(0, 1, fade);//this.Fade);
                angle = first.Angle * t;
                offset = first.Offset * t;
                return;
            }

            float f;
            if (this.FrameCount == 0)
                f = 0;
            else
                f = this.WarpMode == WarpMode.Loop ? frame % this.FrameCount : frame;

            var count = this.Keyframes.Count;
            for (int i = 0; i < count - 1; i++)
            {
                Keyframe a = this[i], b = this[i + 1];
                if (!(a.Time <= f && f < b.Time))
                    continue;
                float t = 1 - ((b.Time - f) / (float)(b.Time - a.Time));
                angle = b.Interpolation(a.Angle, b.Angle, t);
                offset = new Vector2(b.Interpolation(a.Offset.X, b.Offset.X, t), b.Interpolation(a.Offset.Y, b.Offset.Y, t));
                return;
            }
            // CALL THIS WHEN WEIGHT = 0 INSTEAD?
            //if (this.FinishAction != null)
            //{
            //    FinishAction();
            //    FinishAction = null;
            //}
            angle = this.WarpMode == WarpMode.Loop ? this[0].Angle : this[count - 1].Angle;
            offset = this.WarpMode == WarpMode.Loop ? this[0].Offset : this[count - 1].Offset;
        }
    }
}
