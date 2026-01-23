namespace Start_a_Town_
{
    class StatToolSwingSpeed : StatWorker
    {
        public override float CalculateStat(GameObject obj)
        {
            var tool = obj as Entity;
            var material = tool?.GetMaterial(BoneDefOf.ToolHandle);
            if (material is null)
                return 1;
            //var aa = 20f; // what is this?
            //var density = Math.Max(aa, material.Density); // in case for some reason the material is air
            //                                              //var total = density / 100f; // density should add ticks between each tool hit (NOT POSSIBLE THE WAY I HAVE ANIMATIONS SET UP)
            //var total = aa / density;
            var total = material.Density / 50f;
            total /= obj.Quality.Multiplier;
            return total;
        }
    }
}
