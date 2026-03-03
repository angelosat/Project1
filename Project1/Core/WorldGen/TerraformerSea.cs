using Project1.Core.Materials;
using System.Collections.Generic;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;
using Project1.Framework;
using Project1.Core.Blocks;

namespace Project1.Core.WorldGen
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
            for (int i = 0; i < chunk.Cells.Length; i++)
            {

            //}
            //foreach (var c in chunk.Cells)
            //{

                var cellCoords = Chunk.GetLocalFromIndex(i);
                var c = chunk.Cells[i];
                //var z = c.Z;
                var z = cellCoords.Z;
                float zNormal = z / (float)MapBase.MaxHeight - 0.5f;
                if (z > this.SeaLevel)
                    continue;
                else if (z == this.SeaLevel)
                {
                    if (c.Block == BlockDefOf.Air.Block)
                    {
                        c.Block = BlockDefOf.Fluid.Block;
                        c.Material = MaterialDefOf.Water;
                        continue;
                    }
                }
                else
                {
                    if (c.Block == BlockDefOf.Air.Block)
                    {
                        c.Block = BlockDefOf.Fluid.Block;
                        c.Material = MaterialDefOf.Water;
                        c.BlockData = BlockFluid.GetData(1);
                        continue;
                    }
                }
                if (z == 0)
                    continue;
                if (c.Material != MaterialDefOf.Soil)
                    continue;
                //var cellCoords = c.LocalCoords;
                var soilGradient = zNormal + gradients[cellCoords.ToGlobal(chunk)];
                if (sandThreshold <= soilGradient && soilGradient < landThreshold)
                {
                    c.Block = BlockDefOf.Sand.Block;
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

        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.SeaLevel);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.SeaLevel = r.ReadInt32();
        }
    }
}