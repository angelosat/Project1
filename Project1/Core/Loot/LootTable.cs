using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Net;

namespace Project1.Core.Loot
{
    public class LootTable : List<LootWrapper>
    {
        public LootTable()
        {

        }

        public LootTable(params LootWrapper[] loots)
        {
            this.AddRange(loots);
        }
        public override string ToString()
        {
            string text = "";
            foreach (LootWrapper loot in this)
            {
                text += loot.ToString();
            }
            if (text.Length > 0)
                if (text[text.Length - 1] == '\n')
                    return text.Remove(text.Length - 1);
            return text;
        }
        public IEnumerable<GameObject> Generate(RandomThreaded rand)
        {
            foreach (var l in this)
                foreach (var obj in l.Generate(rand))
                    yield return obj;
        }
      
        public static IEnumerable<GameObject> Generate(Random rand, params LootWrapper[] loot)
        {
            for (int k = 0; k < loot.Length; k++)
            {
                var l = loot[k];
                for (int i = 0; i < l.GetRandomCount(rand); i++)
                {
                    var amount = rand.Next(l.AmountMin, l.AmountMax);
                    var obj = l.Factory(amount);
                    yield return obj;
                }
            }
        }
        public IEnumerable<GameObject> GenerateLoot(RandomThreaded random)
        {
            foreach (var i in this.Generate(random))
                yield return i;
        }
        public void PopLoot(RandomThreaded random, Vector3 startPosition, Vector3 startVelocity)
        {
            foreach (var obj in this.GenerateLoot(random))
                PopLoot(random, obj, startPosition, startVelocity);
        }
        static void PopLoot(RandomThreaded random, GameObject obj, Vector3 startPosition, Vector3 startVelocity)
        {
            double angle = random.NextDouble() * (Math.PI + Math.PI);
            double w = Math.PI / 4f;

            float verticalForce = .3f;// 0.3f;
            float horizontalForce = .1f;
            float x = horizontalForce * (float)(Math.Sin(w) * Math.Cos(angle));
            float y = horizontalForce * (float)(Math.Sin(w) * Math.Sin(angle));
            float z = verticalForce * (float)Math.Cos(w);

            var direction = new Vector3(x, y, z);
            var final = startVelocity + direction;

            obj.Global = startPosition;
            obj.Velocity = final;

            //if (obj.RefId == 0)
            //    obj.SyncInstantiate(this);
            //this.Map.SyncSpawn(obj, startPosition, final);
        }
    }
}
