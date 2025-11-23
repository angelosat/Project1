using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class BlockCounter : Block
    {
        AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];
        public BlockCounter()
            : base("Counter", opaque: false)
        {
            this.HidingAdjacent = false;
            this.Orientations[0] = Atlas.Load("blocks/counters/counter1");
            this.Orientations[1] = Atlas.Load("blocks/counters/counter4");
            this.Orientations[2] = Atlas.Load("blocks/counters/counter3");
            this.Orientations[3] = Atlas.Load("blocks/counters/counter2");
            this.Variations.Add(this.Orientations.First());
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Human;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[(orientation + (int)cameraRotation) % 4];
        }
    }
}
