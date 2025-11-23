using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.UI
{
    public class ConsoleEntryNew : GroupBox
    {
        static readonly int TimerLength = Ticks.PerSecond * 8;
        int TimerTick = TimerLength;
        static readonly int FadeLength = Ticks.PerSecond;
        int FadeTick = FadeLength;
        float Fade => this.FadeTick / (float)FadeLength;
        public ConsoleEntryNew()
        {
            this.BackgroundColor = Color.Black * .5f;
            //this.TextColorFunc = () => color;
            this.MouseHover = true;
            this.MouseThrough = true;
        }
        float _cachedOpacity;
        public override float Opacity
        {
            get
            {
                float opacity;
                if (this.TimerTick > 0)
                    opacity = 1;
                else if (this.Fade > 0)
                    opacity = (float)Math.Sin(this.Fade * Math.PI / 2f);
                else
                    opacity = 0;
                this._cachedOpacity =  Math.Max(opacity, Parent.Opacity);
                return this._cachedOpacity;
            }
            set
            {
                this.TimerTick = TimerLength;
                this.FadeTick = FadeLength;
                base.Opacity = value;
                //this.SetMousethrough(false, true);
            }
        }
        public override void Update()
        {
            /// TODO this is bad
            //foreach (var ch in this.Controls) 
            //    ch.SetOpacity(this.Opacity, true);
            base.Update();
            if (this.TimerTick > 0)
            {
                this.TimerTick--;
                return;
            }
            if (this.FadeTick > 0)
            {
                this.FadeTick--;
                //if(this.FadeTick == 0)
                //    this.SetMousethrough(true, true);
                return;
            }
        }
        float childOpacityHelper() => this._cachedOpacity;
        internal override void OnControlAdded(Control control)
        {
            base.OnControlAdded(control);
            control.MouseThrough = this.MouseThrough;
            control.OpacityFunc = childOpacityHelper;
        }
    }
    public class ConsoleEntry : Label
    {
        static readonly int TimerLength = Ticks.PerSecond * 8;
        int TimerTick = TimerLength;
        static readonly int FadeLength = Ticks.PerSecond;
        int FadeTick = FadeLength;

        public ConsoleEntry(Color color, string text) : base(text)
        {
            this.BackgroundColor = Color.Black * .5f;
            this.TextColorFunc = () => color;
            this.MouseHover = true;
            this.MouseThrough = true;
        }

        float Fade => this.FadeTick / (float)FadeLength;
        public override float Opacity
        {
            get
            {
                float opacity;
                if (this.TimerTick > 0)
                    opacity = 1;
                else if (this.Fade > 0)
                    opacity = (float)Math.Sin(this.Fade * Math.PI / 2f);
                else
                    opacity = 0;
                return Math.Max(opacity, Parent.Opacity);
            }
            set
            {
                this.TimerTick = TimerLength;
                this.FadeTick = FadeLength;
                base.Opacity = value;
            }
        }
        public override void Update()
        {
            base.Update();
            if (this.TimerTick > 0)
            {
                this.TimerTick--;
                return;
            }
            if (this.FadeTick > 0)
            {
                this.FadeTick--;
                return;
            }
        }
    }
}
