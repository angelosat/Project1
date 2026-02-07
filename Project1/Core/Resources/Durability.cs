using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Materials;

namespace Project1.Core.Resources
{
    class Durability : ResourceWorker
    {
        public Durability(ResourceDef def) : base(def)
        {
            this.AddThreshold("Durability", 1);
        }
        public override string Description { get; } = "Basic Durability resource";
        public override string Format { get; } = "##0";

        internal override void InitMaterials(Entity obj, Dictionary<string, MaterialDef> materials)
        {
            var count = materials.Count;
            var totaldensity = materials.Values.Sum(m => m.Density);
            var dur = obj.GetResource(this.ResourceDef);
            dur.Initialize(this.BaseMax + totaldensity / count, 1);
            dur.Value = 1; // HACK
        }
        public override Color GetBarColor(Resource resource)
        {
            return Color.LightGray;
        }
        public override string GetBarLabel(Resource resource)
        {
            return $"{resource.Value} / {resource.Max}";
        }
        public override string GetBarHoverText(Resource resource)
        {
            return this.GetLabel(resource);
        }
        //internal override void Revalidate(Resource resource)
        //{
        //    var parent = resource.Owner;
        //    var bones = parent.Body.GetAllBones();
        //    int total = bones.Sum(b => b.Material.Density);
        //    resource.Max = total;
        //}
    }
}
