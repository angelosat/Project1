using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class TerrainManager : TownComponent
    {
        public TerrainManager(Town town) : base(town)
        {
        }
        bool DensityValidated = false;
        float CurrentDensity;
        int TotalFertileCells;
        int TotalFertileCellsTemp;
        public override string Name => "Terrain";
        int CycleIndex;

        Action<MapBase, IntVec3>[] SpawnActions;
        
        PlantProperties[] ValidPlants;
       
        public override void Tick()
        {
            if (this.Map.Net is Net.Client)
                return;
            this.GeneratePlant();
        }
        public void GeneratePlant()
        {
            if(!this.DensityValidated)
            {
                this.TotalFertileCells = this.GetTotalFertileCells();
                this.CurrentDensity = this.GetCurrentPlantDensity(this.TotalFertileCells);
                this.DensityValidated = true;
            }
            var map = this.Map as StaticMap;
            var rand = map.Random;
            var num = this.Map.ActiveChunks.Count;
            for (int i = 0; i < num; i++)
            {
                if(this.CycleIndex >= map.Volume)
                {
                    this.CurrentDensity = this.GetCurrentPlantDensity(this.TotalFertileCellsTemp);
                    this.TotalFertileCellsTemp = 0;
                    this.CycleIndex = 0;
                }
                var global = map.GetNextRandomCell();
                var x = global.X;
                var y = global.Y;
                var z = global.Z;
                var cell = map.GetCell(x, y, z);
                var fertility = cell.Fertility;
                if (fertility > 0 )
                {
                    this.TotalFertileCellsTemp++;
                    if (!this.IsSaturated() && rand.Chance(this.Map.PlantDensityTarget) && this.CanGrowOn(global))
                    {
                        var action = (SpawnActions ??= initSpawnActions()).SelectRandom(rand);
                        action(map, new IntVec3(x, y, z));
                    }
                }
                this.CycleIndex++;
            }

            Action<MapBase, IntVec3>[] initSpawnActions()
            {
                return new Action<MapBase, IntVec3>[] { SpawnEntity, SpawnBlock };
            }
        }

        private void SpawnEntity(MapBase map, IntVec3 global)
        {
            var allPlants = this.ValidPlants ??= this.GetValidPlants();
            var randomPlant = allPlants.SelectRandom(map.Random);
            var plant = randomPlant.CreatePlant();
            plant.SyncInstantiate(map.Net);
            map.SyncSpawn(plant, global.Above, Vector3.Zero);
        }
        private void SpawnBlock(MapBase map, IntVec3 global)
        {
            if (map.GetBlock(global) == BlockDefOf.Grass)
                BlockGrass.GrowRandomFlower(map, global);
        }
        PlantProperties[] GetValidPlants()
        {
            return Def.GetDefs<PlantProperties>().ToArray();
        }
        bool CanGrowOn(IntVec3 global)
        {
            var above = global.Above;
            return
                this.Map.GetSunLight(above) == 15 &&
                !this.Map.GetObjects(above).Any() && 
                this.Map.GetBlock(above) == BlockDefOf.Air;
        }
        bool IsSaturated()
        {
            return this.CurrentDensity >= this.Town.Map.PlantDensityTarget;
        }
       
        private float GetCurrentPlantDensity(int totalFertileCells)
        {
            var plants = this.Map.GetObjects().Where(o => o.IsPlant()).Count();
            return plants / (float)totalFertileCells;
        }

        int GetTotalFertileCells()
        {
            var total = this.Map.GetAllCells().Where(c => c.Fertility > 0).Count();
            return total;
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.DensityValidated.Save("Validated"));
            tag.Add(this.CurrentDensity.Save("CurrentDensity"));
            tag.Add(this.TotalFertileCells.Save("TotalFertileCells"));
            tag.Add(this.TotalFertileCellsTemp.Save("TotalFertileCellsTemp"));
            tag.Add(this.CycleIndex.Save("CycleIndex"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue("Validated", out this.DensityValidated);
            tag.TryGetTagValue("CurrentDensity", out this.CurrentDensity);
            tag.TryGetTagValue("TotalFertileCells", out this.TotalFertileCells);
            tag.TryGetTagValue("TotalFertileCellsTemp", out this.TotalFertileCellsTemp);
            tag.TryGetTagValue("CycleIndex", out this.CycleIndex);
        }
    }
}
