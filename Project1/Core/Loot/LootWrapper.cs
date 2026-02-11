using System;
using System.Collections.Generic;
using System.Diagnostics;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Core.Net;

namespace Project1.Core.Loot
{
    public class LootWrapper
    {
        public int Count, ObjID, AmountMin = 1, AmountMax = 1;
        public float Chance;
        public Func<int, GameObject> Factory;
        public ItemDef ItemDef;
        public GameObject GenerateNew(Random rand)
        {
            var stacksize = rand.Next(this.AmountMin, this.AmountMax);
            var obj = this.Factory(stacksize);
            return obj;
        }
        public LootWrapper(Func<int, GameObject> factory, float chance, int count, int amount) : this(factory, chance, count, amount, amount)
        {
        }
        public LootWrapper(ItemDef def, float chance = 1, int count = 1, int amountmin = 1, int amountmax = 1) : this(x=>def.Create(amount: x), chance, count, amountmin, amountmax)
        {
        }
        public LootWrapper(Func<int, GameObject> factory, float chance, int count, int stackmin, int stackmax)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
            this.AmountMin = stackmin;
            this.AmountMax = stackmax;
        }
        public LootWrapper(Func<int, GameObject> factory, float chance, int count)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
        }
        public LootWrapper(Func<int, GameObject> factory)
            : this(factory, 1, 1)
        {
        }
        public int GetRandomCount(RandomThreaded random)
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (random.NextDouble() < Chance)
                    count++;
            }
            return count;
        }
        public int GetRandomCount(Random random)
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (random.NextDouble() < Chance)
                    count++;
            }
            return count;
        }
       
        internal IEnumerable<GameObject> Generate(RandomThreaded rand)
        {
            if (this.ItemDef is not null && this.AmountMin > this.ItemDef.StackCapacity)
            {
                var amount = rand.Next(this.AmountMin, this.AmountMax);
                var amountRemaining = amount;
                amountRemaining.ToConsole();
                var cap = this.ItemDef.StackCapacity;
                var count = amountRemaining <= cap ? 1 : 1 + amountRemaining / cap;
                var minPerItem = (amount - cap) / (count - 1);
                for (int i = 0; i < count; i++)
                {
                    int allocated = 0;
                    if (i < count - 1)
                    {
                        allocated = rand.Next(minPerItem, cap);
                        amountRemaining -= allocated;
                    }
                    else
                    {
                        Debug.Assert(amountRemaining <= cap);
                        allocated = amountRemaining;
                    }
                    var obj = this.Factory(allocated);

                    yield return obj;
                }
            }
            else
            {
                for (int i = 0; i < this.GetRandomCount(rand); i++)
                {
                    var allocated = rand.Next(this.AmountMin, this.AmountMax);
                    var obj = this.Factory(allocated);
                    yield return obj;
                }
            }
        }
    }
}
