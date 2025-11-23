namespace Start_a_Town_
{
    class AttributeStrength : AttributeWorker
    {
        public AttributeStrength(AttributeDef def) : base(def)
        {
        }

        public override void Tick(GameObject obj, AttributeStat attributeStat)
        {
            var enc = StatDefOf.Encumberance.GetValue(obj);
            this.Award(obj, attributeStat, enc);
        }
        internal override void Award(GameObject obj, AttributeStat attributeStat, float p)
        {
            var stamina = obj.Resources[ResourceDefOf.Stamina];
            var strAwardMultiplier = 1 + (int)(stamina.ResourceDef.Worker.Thresholds.Count * (1 - stamina.CurrentThreshold.Value));
            attributeStat.AddToProgress(strAwardMultiplier * p);
        }
    }
}
