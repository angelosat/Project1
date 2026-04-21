using Microsoft.Xna.Framework;

namespace Project1.Core.Resources
{
    class ResourceWorkerPassive(ResourceDef resourceDef) : ResourceWorker(resourceDef)
    {
        public override string Description => "<placeholder>";
        //public override Color GetBarColor(Resource resource) => Color.LightGray;
        public override Color GetBarColor(ResourceRuntime resource) => resource.Def.Color;
        public override string GetBarLabel(ResourceRuntime resource) => resource.Def.LabelReadable;
    }
}