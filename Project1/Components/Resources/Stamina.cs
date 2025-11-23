using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Resources
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

        public override void Add(float add, Resource resource)
        {
            if (add < 0)
                resource.Rec.Value = 0;
            base.Add(add, resource);
        }
       
        public float TickRate = Ticks.PerGameMinute / 2f; // 2 ticks per second
        public float Timer = 0;
        public float RegenerationRate = 1;

        public override bool HandleMessage(Resource resource, GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Jumped:
                    resource.Rec.Value = resource.Rec.Max;
                    return true;

                default:
                    return base.HandleMessage(resource, parent, e);
            }
        }

        public override void Tick(GameObject parent, Resource values)
        {
            base.Tick(parent, values);
            if (values.Rec.Value < values.Rec.Max)
            {
                values.Rec.Value++;
                return;
            }
            this.Add(this.GetRegenRate(values), values);
        }
        float GetRegenRate(Resource values)
        {
            float rate = (1 + (float)Math.Pow(values.Percentage, 2)) / TickRate;
            return rate;
        }

        public override Color GetBarColor(Resource resource)
        {
            return Color.Yellow;
        }
        public override Control GetControl(Resource res)
        {
            var box = new GroupBox();
            var bar = base.GetControl(res);
            var bar_StaminaRec = new Bar() { Object = res.Rec, Location = bar.BottomLeft, Height = 2 };
            box.AddControls(bar, bar_StaminaRec);
            return box;
        }
    }
}
