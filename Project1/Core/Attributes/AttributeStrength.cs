using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Stats;

namespace Project1.Core.Attributes
{
    sealed class AttributeStrength : AttributeWorker
    {
        public override void Tick(Entity obj, AttributeRuntime attributeStat)
        {
            var enc = StatDefOf.Encumberance.CalculateFor(obj);
            this.Award(obj, attributeStat, enc);
        }
        internal override void Award(Entity obj, AttributeRuntime attributeStat, float p)
        {
            var stamina = obj.Resources.View(ResourceDefOf.Stamina);
            var strAwardMultiplier = 1 + (int)(stamina.Def.Worker.Thresholds.Count * (1 - stamina.CurrentThreshold.Value));
            attributeStat.AddToProgress(strAwardMultiplier * p);
        }
    }
}
