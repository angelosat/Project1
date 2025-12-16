using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
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
                        var plant = randomPlant.Create(PlantFormDefOf.Plant);
                        var comp = plant.GetComponent<PlantComponent>();
                        comp.GrowthBody.Percentage = 1;
                        comp.GrowthFruit.Percentage = 1;
                        int gx = x + (int)chunk.Start.X, gy = y + (int)chunk.Start.Y;
                        plant.Global = new Vector3(gx, gy, z + 1);
                        chunk.Add(plant);
                    }
                }
            }
        }
       
        PlantSpeciesDef[] GetValidPlants()
        {
            return Start_a_Town_.Def.GetDefs<PlantSpeciesDef>().ToArray();
        }
    }
}
