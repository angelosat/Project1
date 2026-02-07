using Project1.Core.Entities;
using System;

namespace Project1.Core.Legacy.Properties
{
    public class ConsumableProperties
    {
        public FoodClass[] FoodClasses;
        internal Func<Entity, Entity> Byproduct;
    }
}
