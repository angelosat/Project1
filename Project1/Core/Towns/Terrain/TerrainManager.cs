using Microsoft.Xna.Framework;
using Project1.Core.Plants;
using Project1.Core.Towns;
using Project1.Core.Base;
using Project1.Core.Components.Plants;
using Project1.Core.Helpers;
using Project1.Core.Net;
using Project1.Core;
using System;
using System.Linq;
using Project1.Core.Net;
using Project1.Core.Simulation;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Terrain
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
        
        PlantSpeciesDef[] ValidPlants;
       
        public override void Tick()
        {
            if ((this.Map.Net is Client))
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
                    if (!this.IsSaturated() && rand.Roll(this.Map.PlantDensityTarget) && this.CanGrowOn(global))
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
            var plant = randomPlant.Create(PlantStageDefOf.Plant);
            map.World.Register(plant);
            map.Spawn(plant, global.Above, Vector3.Zero);
        }
        private void SpawnBlock(MapBase map, IntVec3 global)
        {
            if (map.GetBlock(global) == BlockDefOf.Grass.Worker)
                BlockGrass.GrowRandomFlower(map, global);
        }
        PlantSpeciesDef[] GetValidPlants()
        {
            return Def.GetDefs<PlantSpeciesDef>().ToArray();
        }
        bool CanGrowOn(IntVec3 global)
        {
            var above = global.Above;
            return
                this.Map.GetSunLight(above) == 15 &&
                !this.Map.GetObjects(above).Any() && 
                this.Map.GetBlock(above) == BlockDefOf.Air.Worker;
        }
        bool IsSaturated()
        {
            return this.CurrentDensity >= this.Town.Map.PlantDensityTarget;
        }
       
        private float GetCurrentPlantDensity(int totalFertileCells)
        {
            var plants = this.Map.GetEntities().Where(o => o.IsPlant()).Count();
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
            tag.TryGetTagValueOrDefault("Validated", out this.DensityValidated);
            tag.TryGetTagValueOrDefault("CurrentDensity", out this.CurrentDensity);
            tag.TryGetTagValueOrDefault("TotalFertileCells", out this.TotalFertileCells);
            tag.TryGetTagValueOrDefault("TotalFertileCellsTemp", out this.TotalFertileCellsTemp);
            tag.TryGetTagValueOrDefault("CycleIndex", out this.CycleIndex);
        }
    }
}
