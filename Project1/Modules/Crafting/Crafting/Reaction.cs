using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    partial class Reaction : Def, IListable
    {
        public float Fuel;
        public List<Reagent> Reagents = new();
        public List<Product> Products = new();
        public SkillDef CraftSkill;
        public List<IsWorkstation.Types> ValidWorkshops = new();
        public JobDef Labor;
        public bool CreatesUnfinishedItem;

        public Reaction(string name, List<IsWorkstation.Types> sites, List<Reagent> reagents, List<Product> products, SkillDef skill, JobDef labor = null) : base(name)
        {
            this.ValidWorkshops = sites;
            this.Reagents = reagents;
            this.Products = products;
            this.Labor = labor;
            this.CraftSkill = skill;
        }
        public Reaction(string name, SkillDef skill, JobDef labor = null) : base(name)
        {
            this.Labor = labor;
            this.CraftSkill = skill;
        }
        public Reaction(string name, JobDef labor, IsWorkstation.Types[] sites) : base(name)
        {
            this.ValidWorkshops = sites.ToList();
            this.Labor = labor;
        }

        public Ingredient GetIngredient(string name)
        {
            return this.Reagents.First(r => r.Name == name).Ingredient;
        }

        public Reaction AddIngredient(Ingredient ingredient)
        {
            this.Reagents.Add(new Reagent(ingredient.Name, ingredient));
            return this;
        }
        public Reaction AddIngredients(IEnumerable<Ingredient> ingredients)
        {
            foreach (var i in ingredients)
                this.Reagents.Add(new Reagent(i.Name, i));
            return this;
        }
        public Reaction AddIngredient(string ingredientName, Ingredient ingredient)
        {
            this.Reagents.Add(new Reagent(ingredientName, ingredient));
            return this;
        }

        public Reaction AddProduct(IEnumerable<Product> products)
        {
            foreach (var p in products)
                this.Products.Add(p);
            return this;
        }
        public Reaction AddProduct(params Product[] products)
        {
            foreach (var p in products)
                this.Products.Add(p);
            return this;
        }

        public Reaction AddBuildSite(params IsWorkstation.Types[] sites)
        {
            foreach (var s in sites)
                this.ValidWorkshops.Add(s);
            return this;
        }

        static public List<IsWorkstation.Types> CanBeMadeAt(params IsWorkstation.Types[] blocks)
        {
            return new List<IsWorkstation.Types>(blocks);
        }
        static public void Initialize()
        {

        }

        public Control GetListControlGui()
        {
            return new Label(this.Label);
        }
        int BaseWork = 10;// 100;
        readonly List<(string name, WorkAmountGetter getter)> WorkGetters = new();
        internal delegate int WorkAmountGetter(int baseAmount, GameObject material);
        internal Reaction GetWorkRequiredFromMaterial(string materialName, WorkAmountGetter workGetter)
        {
            this.WorkGetters.Add((materialName, workGetter));
            return this;
        }
        internal Reaction ModWorkRequiredFromMaterials()
        {
            foreach (var r in this.Reagents)
                this.WorkGetters.Add((r.Name, (w, i) => w *= i.PrimaryMaterial.Density));
            return this;
        }
        internal Reaction ModWorkRequiredFromMaterial(string materialName)
        {
            this.WorkGetters.Add((materialName, (w, i) => w *= i.PrimaryMaterial.Density));
            return this;
        }
        internal Reaction SetBaseWork(int baseWork)
        {
            this.BaseWork = baseWork;
            return this;
        }
        private int GetWorkAmount(Dictionary<string, ObjectAmount> ingredients)
        {
            var work = this.BaseWork;
            foreach (var (material, getter) in this.WorkGetters)
                //work = getter(work, ingredients[material].Object);
                work += getter(this.BaseWork, ingredients[material].Object);
            return work;
        }

        public Control GetInfoGui()
        {
            var box = new GroupBox();
            box.AddControlsVertically(this.Reagents.Select(r => r.GetGui()));
            return box;
        }
    }
}
