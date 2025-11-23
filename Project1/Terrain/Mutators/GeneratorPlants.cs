using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class GeneratorPlants : Terraformer
    {
        PlantProperties[] ValidPlants;
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
                    if (rand.Chance(0.5f))
                        continue;
                    var x = rand.Next(0, size);
                    var y = rand.Next(0, size);
                    var z = chunk.HeightMap[x][y];
                    var cell = chunk.GetLocalCell(x, y, z);

                    if (
                        cell.Block == BlockDefOf.Grass)
                    {
                        var allPlants = this.ValidPlants ??= this.GetValidPlants();
                        var randomPlant = allPlants.SelectRandom(map.Random);
                        var plant = randomPlant.CreatePlant();
                        plant.GrowthBody = 1;
                        plant.GrowthFruit = 1;
                        int gx = x + (int)chunk.Start.X, gy = y + (int)chunk.Start.Y;
                        plant.Global = new Vector3(gx, gy, z + 1);
                        chunk.Add(plant);
                    }
                }
            }
        }
       
        PlantProperties[] GetValidPlants()
        {
            return Start_a_Town_.Def.GetDefs<PlantProperties>().ToArray();
        }
    }
}
