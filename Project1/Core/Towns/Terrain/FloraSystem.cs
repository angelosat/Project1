using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Components.Plants;
using Project1.Core.Plants;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Terrain
{
    internal class FloraSystem(MapBase map) : SimulationSystem(map)
    {
        bool DensityValidated;
        float CurrentDensity;
        int TotalFertileCells;
        int TotalFertileCellsTemp;
        int CycleIndex;

        Action<MapBase, IntVec3>[] SpawnActions => field ??= [SpawnEntity, SpawnBlock];

        PlantSpeciesDef[] AllSpieces => field ??= [.. Def.GetDefs<PlantSpeciesDef>()];

        public override void Tick()
        {
            if (this.Map.Net.IsClient)
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
                        var action = this.SpawnActions.SelectRandom(rand);
                        action(map, new IntVec3(x, y, z));
                    }
                }
                this.CycleIndex++;
            }
        }

        private void SpawnEntity(MapBase map, IntVec3 global)
        {
            var randomPlant = this.AllSpieces.SelectRandom(map.Random);
            var plant = randomPlant.Create(PlantStageDefOf.Plant);
            map.World.Register(plant);
            map.Spawn(plant, global.Above, Vector3.Zero);
        }

        private void SpawnBlock(MapBase map, IntVec3 global)
        {
            if (map.GetBlock(global) == BlockDefOf.Grass.Block)
                BlockGrass.GrowRandomFlower(map, global);
        }
        
        bool CanGrowOn(IntVec3 global)
        {
            var above = global.Above;
            return
                this.Map.GetSunLight(above) == 15 &&
                !this.Map.GetEntitiesAt(above).Any() && 
                this.Map.GetBlock(above) == BlockDefOf.Air.Block;
        }

        bool IsSaturated()
        {
            return this.CurrentDensity >= this.Map.PlantDensityTarget;
        }
       
        private float GetCurrentPlantDensity(int totalFertileCells)
        {
            var plants = this.Map.Entities.Where(o => o.IsPlant()).Count();
            return plants / (float)totalFertileCells;
        }

        int GetTotalFertileCells()
        {
            var total = this.Map.GetAllCells().Where(c => c.Fertility > 0).Count();
            return total;
        }
        //protected override void AddSaveData(SaveTag tag)
        //{
        //    tag.Add(this.DensityValidated.Save("Validated"));
        //    tag.Add(this.CurrentDensity.Save("CurrentDensity"));
        //    tag.Add(this.TotalFertileCells.Save("TotalFertileCells"));
        //    tag.Add(this.TotalFertileCellsTemp.Save("TotalFertileCellsTemp"));
        //    tag.Add(this.CycleIndex.Save("CycleIndex"));
        //}
        //public override void Load(SaveTag tag)
        //{
        //    tag.TryGetTagValueOrDefault("Validated", out this.DensityValidated);
        //    tag.TryGetTagValueOrDefault("CurrentDensity", out this.CurrentDensity);
        //    tag.TryGetTagValueOrDefault("TotalFertileCells", out this.TotalFertileCells);
        //    tag.TryGetTagValueOrDefault("TotalFertileCellsTemp", out this.TotalFertileCellsTemp);
        //    tag.TryGetTagValueOrDefault("CycleIndex", out this.CycleIndex);
        //}

        IEnumerable<IntVec3> GetFertileCells(Chunk chunk)
        {
            for (int i = 0; i < Chunk.Size; i++)
            {
                for (int j = 0; j < Chunk.Size; j++)
                {
                    for (int z = MapBase.MaxHeight - 1; z > 0 ; z--)
                    {
                        var local = new IntVec3(i, j, z);
                        var cell = chunk.GetLocalCell(local);
                        if (cell.Block.GetFertility(cell) > 0)
                            yield return local;
                    }
                }
            }
        }
    }
}
