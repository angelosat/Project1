using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Loot;
using Project1.Core.Resources;
using Project1.Core.Systems.Materials;
using Project1.Core.Towns;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.World.WorldAreas
{
    // todo: instead of loot tables, add finite loot pools, and add regeneration rate of said loot pools
    public class FrontierDef : Def
    {
        readonly Dictionary<ItemDef, Dictionary<MaterialDef, int>> ResourcesNew = new();
        public LootTable LootTable;
        public int LootWeightRawMaterial = 1;
        public int LootWeightEquipment = 1;
        public int LootWeightCurrency = 1;
        public readonly int Tier;
        LootWrapper LootCurrency;
        public FrontierDef(string name, int tier) : base(name)
        {
            this.Tier = tier;
        }

        private void Damage(WorldInhabitantView visitor)
        {
            var min = 1;
            var max = 5;
            var actor = visitor.Actor;
            var rand = visitor.World.Random;
            var dmg = min + rand.Next(max - min);
            //actor.Resources.AdjustAndSync(ResourceDefOf.Health, -dmg);
            actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
            actor.AI.State.Log.Write($"[Lost {dmg} health,{Color.Red}] while exploring {this.Name}");
        }

        private void AwardLoot(WorldInhabitantView visitor)
        {
            var actor = visitor.Actor;
            var world = visitor.World;
            if (actor.Inventory.HasFreeSpace)
                actor.Loot(this.GenerateLoot(world.Random), this);
        }

        internal Entity GenerateLoot(Random rand)
        {
            var (factory, weight) = new (Func<GameObject> factory, int weight)[]
            {
                (()=>GetRandomRawMaterial(rand), this.LootWeightRawMaterial),
                (()=>LootCurrency.GenerateNew(rand), this.LootWeightCurrency)
            }.SelectRandomWeighted(rand, p => p.weight);
            var obj = factory();
            return obj as Entity;
        }
        public FrontierDef AddLoot(ItemDef def, MaterialDef mat, float chance)
        {
            return this;
        }

        internal GameObject GetRandomRawMaterial(Random rand)
        {
            if (!this.ResourcesNew.Any())
                return null;
            var matType = this.ResourcesNew.SelectRandom(rand);
            var mat = matType.Value.SelectRandomWeighted(rand, p => p.Value);
            var obj = matType.Key.CreateFrom(mat.Key);
            return obj;
        }

        internal GameObject TryGenerate(ItemDef def, MaterialDef material, Random rand, float chance)
        {
            if (!this.ResourcesNew.TryGetValue(def, out var found))
                return null;
            if (!found.TryGetValue(material, out var foundChance))
                return null;
            if (!rand.Roll(chance))
                return null;
            return def.CreateFrom(material);
        }
        internal bool CanBeFound(ItemDef def, MaterialDef material, out float weight)
        {
            weight = 0;
            if (!this.ResourcesNew.TryGetValue(def, out var found))
                return false;
            if (!found.TryGetValue(material, out var foundWeight))
                return false;
            var totalWeight = found.Values.Sum();
            weight = foundWeight / totalWeight;
            return true;
        }
       
        public FrontierDef AddLoot(LootWrapper loot)
        {
            this.LootTable.Add(loot);
            return this;
        }
        public FrontierDef AddLootRawMaterial(ItemDef item, params (MaterialDef mat, int weight)[] mats)
        {
            if (this.ResourcesNew.TryGetValue(item, out var array))
                foreach (var mat in mats)
                    array.Add(mat.mat, mat.weight);
            else
                this.ResourcesNew[item] = mats.ToDictionary(p => p.mat, p => p.weight);
               
            return this;
        }
        public FrontierDef AddLootCurrency(int min, int max)
        {
            this.LootCurrency = new LootWrapper(ItemDefOf.Coins, amountmin: min, amountmax: max);
            return this;
        }
    }
}
