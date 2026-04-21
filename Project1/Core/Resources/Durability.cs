using Microsoft.Xna.Framework;

namespace Project1.Core.Resources;

sealed class Durability : ResourceWorker
{
    public Durability(ResourceDef def) : base(def)
    {
        this.AddThreshold("Durability", 1);
    }
    public override string Description { get; } = "Basic Durability resource";
    public override string Format { get; } = "##0";

    //internal override void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials)
    //{
    //    var count = materials.Count;
    //    var totaldensity = materials.Values.Sum(m => m.Density);
    //    var dur = obj.GetResource(this.ResourceDef);
    //    //dur.Initialize(this.BaseMax + totaldensity / count, 1);
    //    dur.Max = this.BaseMax + totaldensity / count;
    //    dur.Percentage = 1;
    //    dur.Value = 1; // HACK
    //}
    public override Color GetBarColor(ResourceRuntime resource)
    {
        return Color.LightGray;
    }
    public override string GetBarLabel(ResourceRuntime resource)
    {
        return $"{resource.Value} / {resource.Max}";
    }
    public override string GetBarHoverText(ResourceRuntime resource)
    {
        return this.GetLabel(resource);
    }
}
