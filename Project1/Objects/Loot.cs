using System;
using System.Collections.Generic;
using System.Diagnostics;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class Loot
    {
        public int Count, ObjID, AmountMin = 1, AmountMax = 1;
        public float Chance;
        public Func<GameObject> Factory;
        public ItemDef ItemDef;
        public GameObject GenerateNew(Random rand)
        {
            var obj = this.Factory();
            var stacksize = rand.Next(this.AmountMin, this.AmountMax);
            obj.StackSize = stacksize;
            return obj;
        }
        public Loot(Func<GameObject> factory, float chance, int count, int amount) : this(factory, chance, count, amount, amount)
        {
        }
        public Loot(ItemDef def, float chance = 1, int count= 1, int amountmin= 1, int amountmax = 1) : this(() => def.Factory(def), chance, count, amountmin, amountmax)
        {
        }
        public Loot(Func<GameObject> factory, float chance, int count, int stackmin, int stackmax)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
            this.AmountMin = stackmin;
            this.AmountMax = stackmax;
        }
        public Loot(Func<GameObject> factory, float chance, int count)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
        }
        public Loot(Func<GameObject> factory)
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
                    var obj = this.Factory();
                    if (i < count - 1)
                    {
                        obj.StackSize = rand.Next(minPerItem, cap);
                        amountRemaining -= obj.StackSize;
                    }
                    else
                    {
                        Debug.Assert(amountRemaining <= cap);
                        obj.StackSize = amountRemaining;
                    }
                    yield return obj;
                }
            }
            else
            {
                for (int i = 0; i < this.GetRandomCount(rand); i++)
                {
                    var obj = this.Factory();
                    var stacksize = rand.Next(this.AmountMin, this.AmountMax);
                    obj.StackSize = stacksize;
                    yield return obj;
                }
            }
        }
    }
}
