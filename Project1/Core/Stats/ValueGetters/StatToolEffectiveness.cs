using Project1.Core.Animations;

namespace Project1.Core.Entities.Stats.ValueGetters
{
    sealed class StatToolEffectiveness : StatWorker
    {
        public override float CalculateStat(Entity obj)
        {
            var tool = obj as Entity;
            var material = tool.GetMaterial(BoneDefOf.ToolHead);
            if (material is null)
                return 1; // is it ever possible for this to be null?
            return material.Density * obj.Quality.Multiplier;
        }
    }
}
