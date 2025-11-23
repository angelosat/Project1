using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    class Transition
    {
        public enum States { Unstarted, Running, Finished }

        public States State { get; private set; }
        public Keyframe A, B;
        int Frames;

        static public Transition Create(Keyframe a, Keyframe b)
        {
            
            return new Transition(a, b);
        }

        Transition(Keyframe a, Keyframe b)
        {
            this.A = a;
            this.B = b;
            this.Frames = b.Time - a.Time;
        }
       
        public float GetPercentage(int frame)
        {
            return 1 - ((this.B.Time - frame) / (float)(this.B.Time - this.A.Time));
        }
        public States Step(int frame, out Vector2 offset, out float angle)
        {
            if (A.Time == B.Time)
            {
                angle = B.Angle;
                offset = B.Offset;
                return States.Running;
            }
            float t = GetPercentage(frame);
            angle = this.B.Interpolation(this.A.Angle, this.B.Angle, t);// this.A.Angle + (this.B.Angle - this.A.Angle) * t;
            offset = new Vector2(this.B.Interpolation(this.A.Offset.X, this.B.Offset.X, t), this.B.Interpolation(this.A.Offset.Y, this.B.Offset.Y, t));
            State = frame >= (this.B.Time - 1) ? States.Finished : States.Running;
            return State;
        }

        public override string ToString()
        {
            return this.A.ToString() + " -> " + this.B.ToString();
        }
    }
}
