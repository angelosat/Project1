namespace Start_a_Town_
{
    class StatToolEffectiveness : StatValueGetter
    {
        public StatToolEffectiveness(StatDef parent) : base(parent)
        {
        }

        public override float GetValue(GameObject obj)
        {
            var tool = obj as Entity;
            var material = tool.GetMaterial(BoneDefOf.ToolHead);
            if (material is null)
                return 1; // is it ever possible for this to be null?
            return material.Density * obj.Quality.Multiplier;
        }
    }
}
