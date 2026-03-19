using System;
using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Framework.Serialization;

namespace Project1.Core.Animations
{
    public struct Keyframe : ISerializableNewNew<Keyframe>
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
            this.Interpolation = Animations.Interpolation.Lerp;
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

        public IDataWriter Write(IDataWriter w)
        {
            w.Write(this.Time);
            w.Write(this.Angle);
            w.Write(this.Offset);
            return w;
        }

        public static Keyframe Create(IDataReader r)
        {
            var time = r.ReadInt32();
            var angle = r.ReadSingle();
            var offset = r.ReadVector2();
            return new(time, offset, angle);
        }
    }
}
