using Project1.Core.Entities;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Legacy.Storage;
using Project1.Core.Skills;
using Project1.Core.Towns.Duties;
using System;
using System.Collections.Generic;

namespace Project1.Core.Legacy.Properties
{
    public class RecipeProperties
    {
        List<Func<ItemDef, Ingredient>> IngredientMakers = new();
        List<Func<ItemDef, Reaction.Product>> ProductMakers = new();

        List<IsWorkstation.Types> Workstations = new();
        public ItemCategory IngredientCategory;
        public string Verb;
        public string IngredientName;
        public DutyDef Job;
        public SkillDef Skill;

        public RecipeProperties(string verb, ItemCategory ingCat)
        {
            this.Verb = verb;
            this.IngredientCategory = ingCat;
        }
        public RecipeProperties(string verb)
        {
            this.Verb = verb;
        }
        
        public RecipeProperties AddIngredientMaker(Func<ItemDef, Ingredient> maker)
        {
            this.IngredientMakers.Add(maker);
            return this;
        }
        
        public RecipeProperties AddProductMaker(Func<ItemDef, Reaction.Product> productMaker)
        {
            this.ProductMakers.Add(productMaker);
            return this;
        }
        public RecipeProperties AddWorkstation(IsWorkstation.Types station)
        {
            this.Workstations.Add(station);
            return this;
        }
        public IEnumerable<Ingredient> MakeIngredients(ItemDef def)
        {
            foreach (var maker in this.IngredientMakers)
                yield return maker(def);
        }
        public IEnumerable<Reaction.Product> MakeProducts(ItemDef def)
        {
            foreach (var maker in this.ProductMakers)
                yield return maker(def);
        }
        public Reaction CreateRecipe(ItemDef def)
        {
            return new Reaction($"{this.Verb} {def.LabelReadable}", this.Job, this.Workstations.ToArray()) { CraftSkill = this.Skill }
                .AddIngredients(this.MakeIngredients(def))
                .AddProduct(this.MakeProducts(def));
        }
    }
}
