using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Stats;

namespace Project1.Core.Resources;

internal class ResourceWorker_Patience : ResourceWorker
{
    public ResourceWorker_Patience(ResourceDef resourceDef) : base(resourceDef)
    {
    }

    internal override float GetMax(Entity owner)
        => StatDefOf.MaxPatience.Worker.CalculateStat(owner);

    public override string Description => "patience resource description";

    public override Color GetBarColor(Resource resource) => Color.Bisque;

    public override string GetBarLabel(Resource resource) => resource.Def.LabelReadable;
}
