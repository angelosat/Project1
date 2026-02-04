using Project1.Framework.Entities;
using System;

namespace Start_a_Town_
{
    public class ConsumableProperties
    {
        public FoodClass[] FoodClasses;
        internal Func<Entity, Entity> Byproduct;
    }
}
