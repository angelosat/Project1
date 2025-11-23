using System.Collections.Generic;

namespace Start_a_Town_.Terraforming.Mutators
{
    class TerraformerSea : Terraformer
    {
        readonly TerraformerProperty SeaLevelProp = new("Sea Level", MapBase.MaxHeight / 2 - 1, 0, MapBase.MaxHeight - 1, 1);
        public int SeaLevel
        {
            get => (int)this.SeaLevelProp.Value;
            set => this.SeaLevelProp.Value = value;
        }
        internal override void Finally(Chunk chunk, Dictionary<IntVec3, double> gradients)
        {
            var sandThickness = .01f;
            var landTerraformer = chunk.Map.World.GetTerraformer<TerraformerNormal>();
            var landThreshold = landTerraformer.GroundAirThreshold;
            var sandThreshold = landThreshold - sandThickness;
            foreach (var c in chunk.Cells)
            {
                var z = c.Z;
                float zNormal = z / (float)MapBase.MaxHeight - 0.5f;
                if (z > this.SeaLevel)
                    continue;
                else if (z == this.SeaLevel)
                {
                    if (c.Block == BlockDefOf.Air)
                    {
                        c.Block = BlockDefOf.Fluid;
                        c.Material = MaterialDefOf.Water;
                        continue;
                    }
                }
                else
                {
                    if (c.Block == BlockDefOf.Air)
                    {
                        c.Block = BlockDefOf.Fluid;
                        c.Material = MaterialDefOf.Water;
                        c.BlockData = BlockFluid.GetData(1);
                        continue;
                    }
                }
                if (z == 0)
                    continue;
                if (c.Material != MaterialDefOf.Soil)
                    continue;
                var cellCoords = c.LocalCoords;
                var soilGradient = zNormal + gradients[cellCoords];
                if (sandThreshold <= soilGradient && soilGradient < landThreshold)
                {
                    c.Block = BlockDefOf.Sand;
                    c.Material = MaterialDefOf.Sand;
                }
            }
        }
      
        public override IEnumerable<TerraformerProperty> GetAdjustableParameters()
        {
            yield return this.SeaLevelProp;
        }
       
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, "Level", this.SeaLevel));
        }
        protected override void LoadExtra(SaveTag save)
        {
            this.SeaLevel = save.TagValueOrDefault("Level", MapBase.MaxHeight / 2 - 1);
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.SeaLevel);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.SeaLevel = r.ReadInt32();
        }
    }
}
