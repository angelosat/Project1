using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Stats;

namespace Project1.Core.Resources;

internal class ResourceWorker_Patience : ResourceWorker
{
    public ResourceWorker_Patience(ResourceDef resourceDef) : base(resourceDef)
    {
    }

    internal override int GetMax(Entity owner)
        => (int)StatDefOf.MaxPatience.Worker.CalculateStat(owner);

    public override string Description => "patience resource description";

    public override Color GetBarColor(ResourceRuntime resource) => Color.Bisque;

    public override string GetBarLabel(ResourceRuntime resource) => resource.Def.LabelReadable;
}
