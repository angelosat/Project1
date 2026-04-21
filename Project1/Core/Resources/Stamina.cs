using System;
using Microsoft.Xna.Framework;
using Project1.Framework.UI;
using Project1.Framework.UI.Primitives;

namespace Project1.Core.Resources
{
    class Stamina : ResourceWorker
    {
        public Stamina(ResourceDef def) : base(def)
        {
            this.AddThreshold("Out of breath", .25f);
            this.AddThreshold("Exhausted", .5f);
            this.AddThreshold("Tired", .75f);
            this.AddThreshold("Energetic", 1f);
        }
        public override string Format { get; } = "##0.00";
        public override string Description { get; } = "Required for sprinting and hauling heavy objects";

        public override void ApplyDelta(ResourceRuntime resource, int delta)
        {
            if (delta < 0)
                resource.RechargingDelay.Value = 0;
            base.ApplyDelta(resource, delta);
        }
       
        public float TickRate = Ticks.PerGameMinute / 2f; // 2 ticks per second
        public float Timer = 0;
        public float RegenerationRate = 1;
        protected override void updateRec(ResourceRuntime resource)
        {
            if (resource.RechargingDelay.Value < resource.RechargingDelay.Max)
            {
                resource.RechargingDelay.Value++;
                return;
            }
        }
        protected override float GetRegenRate(ResourceRuntime values)
        {
            float rate = (1 + (float)Math.Pow(values.Percentage, 2)) / TickRate;
            return rate;
        }

        public override Color GetBarColor(ResourceRuntime resource)
        {
            return Color.Yellow;
        }
        public override Control GetControlBar(ResourceRuntime res)
        {
            var box = new GroupBox();
            var bar = base.GetControlBar(res);
            var bar_StaminaRec = new Bar() { Object = res.RechargingDelay, Location = bar.BottomLeft, Height = 2 };
            box.AddControls(bar, bar_StaminaRec);
            return box;
        }
    }
}
