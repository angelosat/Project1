using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class MaterialTemplates
    {
        static public readonly MaterialDef Fruit = new MaterialDef()
            { 
                EdibleRaw = true,
                EdibleCooked = true,
                State = MaterialState.Solid,
                Density = 2
            }.SetType(MaterialTypeDefOf.Fruit)
            ;

        static public readonly MaterialDef Wood = new MaterialDef()
            { Fuel = new Fuel(FuelDef.Organic, 20f) }
            .SetState(MaterialState.Solid)
            .SetDensity(50)
            .SetType(MaterialTypeDefOf.Wood)
            ;

        static public readonly MaterialDef Metal = new MaterialDef()
            .SetState(MaterialState.Solid)
            .SetDensity(100)
            .SetReflectiveness(1)
            .SetType(MaterialTypeDefOf.Metal)
            ;

        static public readonly MaterialDef Stone = new MaterialDef()
            .SetState(MaterialState.Solid)
            .SetDensity(75)
            .SetType(MaterialTypeDefOf.Stone)
            ;

        static public readonly MaterialDef Soil = new MaterialDef()
          .SetState(MaterialState.Solid)
          .SetDensity(10)
          .SetType(MaterialTypeDefOf.Soil)
          ;

        static public readonly MaterialDef Meat = new MaterialDef()
          {
              EdibleRaw = true,
              EdibleCooked = true
          }
          .SetState(MaterialState.Solid)
          .SetDensity(20)
          .SetColor(Color.LightPink)
          .SetType(MaterialTypeDefOf.Meat)
          ;

        static public readonly MaterialDef PlantStem = new MaterialDef()
            .SetDensity(30)
            .SetState(MaterialState.Solid)
            .SetType(MaterialTypeDefOf.PlantStem);

        static public readonly MaterialDef Seed = new MaterialDef()
            .SetDensity(40)
            .SetState(MaterialState.Solid)
            .SetType(MaterialTypeDefOf.Seed);

    }
}
