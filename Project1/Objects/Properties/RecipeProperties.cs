using Start_a_Town_.Components.Crafting;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class RecipeProperties
    {
        List<Func<ItemDef, Ingredient>> IngredientMakers = new();
        List<Func<ItemDef, Reaction.Product>> ProductMakers = new();

        List<IsWorkstation.Types> Workstations = new();
        public ItemCategory IngredientCategory;
        public string Verb;
        public string IngredientName;
        public JobDef Job;
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
            return new Reaction($"{this.Verb} {def.Label}", this.Job, this.Workstations.ToArray()) { CraftSkill = this.Skill }
                .AddIngredients(this.MakeIngredients(def))
                .AddProduct(this.MakeProducts(def));
        }
    }
}
