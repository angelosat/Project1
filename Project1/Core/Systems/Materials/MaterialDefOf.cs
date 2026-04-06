using Microsoft.Xna.Framework;
using Project1.Framework;

namespace Project1.Core.Systems.Materials
{
    [EnsureStaticCtorCall]
    static class MaterialDefOf
    {
        static public readonly MaterialDef Copper = new MaterialDef("Copper", MaterialTemplates.Metal)
        {
            Shine = .9f,
            BreakResistance = 30,
            Color = Color.IndianRed,
            Tier = 1
        }
          .SetPrefix("Copper")
          .SetValue(20);
        static public readonly MaterialDef Iron = new MaterialDef("Iron", MaterialTemplates.Metal)
        {
            BreakResistance = 30,
            Color = Color.LightSteelBlue,
            Tier = 2
        }
            .SetPrefix("Iron")
            .SetValue(40);
        static public readonly MaterialDef Cobalt = new("Cobalt", MaterialTemplates.Metal)
        {
            Shine = .7f,
            ValueBase = 60,
            Color = Color.DodgerBlue,
            Density = 50,
            BreakResistance = 30,
            Tier = 3
        };
        static public readonly MaterialDef Silver = new MaterialDef("Silver", MaterialTemplates.Metal)
        {
            Shine = 1,
            BreakResistance = 30,
            Color = Color.White,
            Tier = 4
        }
            .SetPrefix("Silver")
            .SetValue(80);
        static public readonly MaterialDef Gold = new MaterialDef("Gold", MaterialTemplates.Metal) 
        { 
            Shine = 1,
            Color = Color.Gold,
            Tier = 5
        }
            .SetPrefix("Golden")
            .SetValue(100);

        static public readonly MaterialDef Coal = new(MaterialTypeDefOf.FossilFuel/*Stone*/, "Coal", "Coal", Color.DimGray, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), BreakResistance = 25, Tier = 1 };
  
        static public readonly MaterialDef Stone = new(MaterialTypeDefOf.Stone, "Stone", "Stone", /*Color.DimGray*/Color.Beige, 80) { ValueBase = 5, BreakResistance = 20, Tier = 1 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite
        static public readonly MaterialDef Limestone = new(MaterialTypeDefOf.Stone, "Limestone", "Limestone", Color.Beige, 80) { ValueBase = 5, BreakResistance = 20, Tier = 2 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite
        static public readonly MaterialDef Granite = new(MaterialTypeDefOf.Stone, "Granite", "Granite", Color.LightSlateGray, 80) { ValueBase = 5, BreakResistance = 20, Tier = 3 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite

        static public readonly MaterialDef Diamond = new(MaterialTypeDefOf.Crystal, "Diamond", "Diamond", Color.AliceBlue, 80) { ValueBase = 5, BreakResistance = 20, Tier = 3 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite
        static public readonly MaterialDef Ruby = new(MaterialTypeDefOf.Crystal, "Ruby", "Ruby", Color.Red, 80) { ValueBase = 5, BreakResistance = 20, Tier = 2 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite
        static public readonly MaterialDef Topaz = new(MaterialTypeDefOf.Crystal, "Topaz", "Topaz", Color.Goldenrod, 80) { ValueBase = 5, BreakResistance = 20, Tier = 1 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite

        //static public readonly MaterialDef CoalNew = new(MaterialTypeDefOf.FossilFuel, "CoalNew", "CoalNew", Color.Black, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), BreakResistance = 25, Tier = 1 };
        static public readonly MaterialDef Peat = new(MaterialTypeDefOf.FossilFuel, "Peat", "Peat", Color.SaddleBrown, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), BreakResistance = 25, Tier = 2 };
        static public readonly MaterialDef Lignite = new(MaterialTypeDefOf.FossilFuel, "Lignite", "Lignite", Color.DimGray, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), BreakResistance = 25, Tier = 3 };

        static public readonly MaterialDef ShrubStem = new MaterialDef("Twig", MaterialTemplates.PlantStem) { Tier = 1 }
            .SetColor(new Color(139, 136, 95, 255));// Color.DarkOliveGreen

        static public readonly MaterialDef LightWood = new MaterialDef("Light Wood", MaterialTemplates.Wood) { Tier = 1 }
            .SetPrefix("Light Wood")
            .SetColor(Color.SandyBrown)
            .SetValue(5);
        static public readonly MaterialDef DarkWood = new MaterialDef("Dark Wood", MaterialTemplates.Wood) { Tier = 2 }
            .SetPrefix("Dark Wood")
            .SetColor(Color.SaddleBrown)
            .SetValue(10);
        static public readonly MaterialDef RedWood = new MaterialDef("Red Wood", MaterialTemplates.Wood) { Tier = 3 }
            .SetPrefix("Red Wood")
            .SetColor(Color.Brown)
            .SetValue(20);
        static public readonly MaterialDef VineWood = new MaterialDef("Vine Wood", MaterialTemplates.Wood) { Shine = .5f, Tier = 4 }
            .SetPrefix("Vine Wood")
            .SetColor(Color.GreenYellow)
            .SetValue(30);
        static public readonly MaterialDef BlackWood = new MaterialDef("Black Wood", MaterialTemplates.Wood) { Shine = .5f, Tier = 5 }
            .SetPrefix("Black Wood")
            .SetColor(Color.DarkSlateGray)
            .SetValue(40);

        static public readonly MaterialDef Soil = new(MaterialTypeDefOf.Sediment, "Soil", "Dirt", Color.SandyBrown, 20) { ValueBase = 2, BreakResistance = 4, Tier = 1 };
        static public readonly MaterialDef Sand = new(MaterialTypeDefOf.Sediment, "Sand", "Sand", Color.Khaki, 10) { ValueBase = 2, Tier = 2 };
        //static public readonly MaterialDef SandNew = new(MaterialTypeDefOf.Sediment, "SandNew", "SandNew", Color.BlanchedAlmond, 10) { ValueBase = 2 };
        static public readonly MaterialDef Dirt = new(MaterialTypeDefOf.Sediment, "Dirt", "Dirt", Color.SaddleBrown, 10) { ValueBase = 2, Tier = 3 };


        static public readonly MaterialDef Air = new(MaterialTypeDefOf.Gas, "Air", "Air", 0);
        // basalt? new Color(120, 109, 95, 255)
        static public readonly MaterialDef Water = new(MaterialTypeDefOf.Water, "Water", "Water", Color.SeaGreen, 5) { Viscosity = 30 };
        static public readonly MaterialDef Glass = new(MaterialTypeDefOf.Glass, "Glass", "Glass", Color.White, 40);

        static public readonly MaterialDef Human = 
            new MaterialDef("Human", MaterialTemplates.Meat) { Tier = 1 }
            .SetPrefix("Human")
            .SetValue(20);
        static public readonly MaterialDef Animal = 
            new MaterialDef("Animal", MaterialTemplates.Meat) { Tier = 2 }
            .SetPrefix("Animal")
            .SetValue(20);
        static public readonly MaterialDef Insect = 
            new MaterialDef("Insect", MaterialTemplates.Meat) { Tier = 3 }
            .SetPrefix("Insect")
            .SetValue(20);

        static public readonly MaterialDef Berry = 
            new MaterialDef("Berry", MaterialTemplates.Fruit) { Tier = 1 }
            .SetPrefix("Berry")
            //.SetColor(new Color(141, 78, 133));
            .SetColor(Color.MediumVioletRed);

        static public readonly MaterialDef Seed = new("Seed", MaterialTemplates.Seed);

        static MaterialDefOf()
        {
            Def.Register(typeof(MaterialDefOf));
            //Def.Register(Iron);
            //Def.Register(Gold);

            //Def.Register(LightWood);
            //Def.Register(DarkWood);
            //Def.Register(RedWood);

            //Def.Register(Coal);
            //Def.Register(Stone);

            //Def.Register(ShrubStem);

            //Def.Register(Soil);
            //Def.Register(Sand);

            //Def.Register(Air);
            //Def.Register(Water);
            //Def.Register(Glass);

            //Def.Register(Human);
            //Def.Register(Animal);
            //Def.Register(Insect);

            //Def.Register(Berry);
            //Def.Register(Seed);
        }
    }
}
