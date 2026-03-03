using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Components.Plants;
using Project1.Core.Plants;
using Project1.Core.Simulation;
using Project1.Framework.Helpers;
using System.Linq;

namespace Project1.Core.WorldGen
{
    class GeneratorPlants : Terraformer
    {
        PlantSpeciesDef[] ValidPlants;
        public GeneratorPlants()
        {
        }
        public override void Generate(MapBase map)//, Dictionary<IntVec3, double> gradients)
        {
            var size = Chunk.Size;
            var rand = map.Random;
            foreach (var chunk in map.ActiveChunks.Values)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (rand.Roll(0.5f))
                        continue;
                    var x = rand.Next(0, size);
                    var y = rand.Next(0, size);
                    var z = chunk.HeightMap[x][y];
                    var cell = chunk.GetLocalCell(x, y, z);

                    if (
                        cell.Block == BlockDefOf.Grass.Block)
                    {
                        var allPlants = this.ValidPlants ??= this.GetValidPlants();
                        var randomPlant = allPlants.SelectRandom(map.Random);
                        var plant = randomPlant.Create(PlantStageDefOf.Plant);
                        var comp = plant.GetComponent<PlantComponent>();
                        comp.GrowthBody.Percentage = 1;
                        comp.GrowthFruit.Percentage = 1;
                        int gx = x + (int)chunk.Start.X, gy = y + (int)chunk.Start.Y;
                        plant.Global = new Vector3(gx, gy, z + 1);
                        chunk.World.Register(plant);
                        chunk.Add(plant);
                    }
                }
            }
        }
       
        PlantSpeciesDef[] GetValidPlants()
        {
            return Core.Def.GetDefs<PlantSpeciesDef>().ToArray();
        }
    }
}
