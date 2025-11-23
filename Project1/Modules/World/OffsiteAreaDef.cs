using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class OffsiteAreaDef : Def
    {
        readonly Dictionary<ItemDef, Dictionary<MaterialDef, int>> ResourcesNew = new();
        public LootTable LootTable;
        public int LootWeightRawMaterial = 1;
        public int LootWeightEquipment = 1;
        public int LootWeightCurrency = 1;
        readonly Action<VisitorProperties>[] TickActions;
        Loot LootCurrency;
        public OffsiteAreaDef(string name) : base(name)
        {
            this.TickActions = new Action<VisitorProperties>[] {
                AwardLoot,
                Quest,
                Damage
            };
        }
        public void Tick(VisitorProperties props)
        {
            this.TickActions.SelectRandomWeighted(props.World.Random, p => 1)(props);
        }

        private void Damage(VisitorProperties visitor)
        {
            var min = 1;
            var max = 5;
            var actor = visitor.Actor;
            var rand = visitor.World.Random;
            var dmg = min + rand.Next(max - min);
            actor.SyncAdjustResource(ResourceDefOf.Health, -dmg);
            AILog.SyncWrite(actor, $"[Lost {dmg} health,{Color.Red}] while exploring {this.Name}");
        }

        private void Quest(VisitorProperties visitor)
        {
            var actor = visitor.Actor;
            foreach (var q in visitor.GetQuests())
                q.TryComplete(actor, this);
        }

        private void AwardLoot(VisitorProperties visitor)
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
                //(()=>GetRandomEquipment(rand), this.LootWeightRawMaterial),
                (()=>LootCurrency.GenerateNew(rand), this.LootWeightCurrency)
            }.SelectRandomWeighted(rand, p => p.weight);
            var obj = factory();
            return obj as Entity;
        }
        public OffsiteAreaDef AddLoot(ItemDef def, MaterialDef mat, float chance)
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
            if (!rand.Chance(chance))
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
       
        public OffsiteAreaDef AddLoot(Loot loot)
        {
            this.LootTable.Add(loot);
            return this;
        }
        public OffsiteAreaDef AddLootRawMaterial(ItemDef item, params (MaterialDef mat, int weight)[] mats)
        {
            if (this.ResourcesNew.TryGetValue(item, out var array))
                foreach (var mat in mats)
                    array.Add(mat.mat, mat.weight);
            else
                this.ResourcesNew[item] = mats.ToDictionary(p => p.mat, p => p.weight);
               
            return this;
        }
        public OffsiteAreaDef AddLootCurrency(int min, int max)
        {
            this.LootCurrency = new Loot(ItemDefOf.Coins, amountmin: min, amountmax: max);
            return this;
        }
    }
}
