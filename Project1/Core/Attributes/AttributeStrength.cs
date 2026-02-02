using Project1.Framework.Attributes;
using Project1.Framework.Resources;
using Project1.Framework.Stats;
using Start_a_Town_;

namespace Project1.Core.Attributes
{
    class AttributeStrength : AttributeWorker
    {
        public AttributeStrength(AttributeDef def) : base(def)
        {
        }

        public override void Tick(GameObject obj, AttributeRuntime attributeStat)
        {
            var enc = StatDefOf.Encumberance.CalculateFor(obj);
            this.Award(obj, attributeStat, enc);
        }
        internal override void Award(GameObject obj, AttributeRuntime attributeStat, float p)
        {
            var stamina = obj.Resources[ResourceDefOf.Stamina];
            var strAwardMultiplier = 1 + (int)(stamina.ResourceDef.Worker.Thresholds.Count * (1 - stamina.CurrentThreshold.Value));
            attributeStat.AddToProgress(strAwardMultiplier * p);
        }
    }
}
