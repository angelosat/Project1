using Project1.Framework.Animations;
using Project1.Framework.Entities;

namespace Project1.Framework.Stats.ValueGetters
{
    class StatToolEffectiveness : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var tool = obj as Entity;
            var material = tool.GetMaterial(BoneDefOf.ToolHead);
            if (material is null)
                return 1; // is it ever possible for this to be null?
            return material.Density * obj.Quality.Multiplier;
        }
    }
}
