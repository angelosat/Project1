using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class MaterialDefOf
    {
        internal static void Init()
        {
        }

        static public readonly MaterialDef Iron = new MaterialDef("Iron", MaterialTemplates.Metal)
            .SetPrefix("Iron")
            .SetColor(Color.LightSteelBlue)
            .SetValue(20);
        static public readonly MaterialDef Gold = new MaterialDef("Gold", MaterialTemplates.Metal)
            .SetPrefix("Golden")
            .SetColor(Color.Gold)
            .SetValue(100);


        static public readonly MaterialDef Coal = new(MaterialTypeDefOf.Stone, "Coal", "Coal", Color.DimGray, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), BreakResistance = 25 };
        static public readonly MaterialDef Stone = new(MaterialTypeDefOf.Stone, "Stone", "Stone", Color.White, 80) { ValueBase = 5, BreakResistance = 20 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite

        static public readonly MaterialDef ShrubStem = new MaterialDef("Twig", MaterialTemplates.PlantStem)
            .SetColor(new Color(139, 136, 95, 255));// Color.DarkOliveGreen

        static public readonly MaterialDef LightWood = new MaterialDef("Light Wood", MaterialTemplates.Wood)
            .SetPrefix("Light Wood")
            .SetColor(Color.SandyBrown)
            .SetValue(5);
        static public readonly MaterialDef DarkWood = new MaterialDef("Dark Wood", MaterialTemplates.Wood)
            .SetPrefix("Dark Wood")
            .SetColor(Color.SaddleBrown)
            .SetValue(10);
        static public readonly MaterialDef RedWood = new MaterialDef("Red Wood", MaterialTemplates.Wood)
            .SetPrefix("Red Wood")
            .SetColor(Color.Brown)
            .SetValue(20);


        static public readonly MaterialDef Soil = new(MaterialTypeDefOf.Soil, "Soil", "Dirt", Color.SandyBrown, 20) { ValueBase = 2, BreakResistance = 20 };
        static public readonly MaterialDef Sand = new(MaterialTypeDefOf.Soil, "Sand", "Sand", Color.BlanchedAlmond, 10) { ValueBase = 2 };


        static public readonly MaterialDef Air = new(MaterialTypeDefOf.Gas, "Air", "Air", 0);
        // basalt? new Color(120, 109, 95, 255)
        static public readonly MaterialDef Water = new(MaterialTypeDefOf.Water, "Water", "Water", Color.SeaGreen, 5) { Viscosity = 30 };
        static public readonly MaterialDef Glass = new(MaterialTypeDefOf.Glass, "Glass", "Glass", Color.White, 40);

        static public readonly MaterialDef Human = 
            new MaterialDef("Human", MaterialTemplates.Meat)
            .SetPrefix("Human")
            .SetValue(20);
        static public readonly MaterialDef Animal = 
            new MaterialDef("Animal", MaterialTemplates.Meat)
            .SetPrefix("Animal")
            .SetValue(20);
        static public readonly MaterialDef Insect = 
            new MaterialDef("Insect", MaterialTemplates.Meat)
            .SetPrefix("Insect")
            .SetValue(20);

        static public readonly MaterialDef Berry = 
            new MaterialDef("Berry", MaterialTemplates.Fruit)
            .SetPrefix("Berry")
            //.SetColor(new Color(141, 78, 133));
            .SetColor(Color.MediumVioletRed);

        static public readonly MaterialDef Seed = new("Seed", MaterialTemplates.Seed);

        static MaterialDefOf()
        {
            Def.Register(Iron);
            Def.Register(Gold);

            Def.Register(LightWood);
            Def.Register(DarkWood);
            Def.Register(RedWood);

            Def.Register(Coal);
            Def.Register(Stone);

            Def.Register(ShrubStem);

            Def.Register(Soil);
            Def.Register(Sand);

            Def.Register(Air);
            Def.Register(Water);
            Def.Register(Glass);

            Def.Register(Human);
            Def.Register(Animal);
            Def.Register(Insect);

            Def.Register(Berry);
            Def.Register(Seed);
        }
    }
}
