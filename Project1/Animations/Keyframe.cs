using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public struct Keyframe
    {
        public int Time;
        public float Angle;
        public Vector2 Offset;
        public Func<float, float, float, float> Interpolation;
        public Action<GameObject> Event;
        public Keyframe(int time, Vector2 offset, float angle, Func<float, float, float, float> interpolation)
        {
            this.Time = time;
            this.Offset = offset;
            this.Angle = angle;
            this.Interpolation = interpolation;
            this.Event = e => { };
        }
        public Keyframe(int time, Vector2 offset, float angle)
        {
            this.Time = time;
            this.Offset = offset;
            this.Angle = angle;
            this.Interpolation = Start_a_Town_.Interpolation.Lerp;
            this.Event = e => { };
        }
        public override string ToString()
        {
            return Time.ToString() + " " + Offset.ToString() + " " + this.Angle.ToString();
        }
        public Keyframe AddEvent(Action<GameObject> action)
        {
            this.Event = action;
            return this;
        }
    }
}
