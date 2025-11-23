using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class Reaction
    {
        public sealed partial class Reagent : ILabeled
        {
            public class ReagentFilter
            {
                public virtual string Name { get; set; }
                public Func<Entity, bool> _Condition;
                public Func<ItemDef, bool> _DefCondition;
                public Func<MaterialDef, bool> _MatCondition;

                public virtual bool Condition(Entity obj)
                {
                    return this._Condition(obj);
                }
            }

            public string Name { get; set; }

            public string Label => Name;
            public Ingredient Ingredient;

            public int Quantity => this.Ingredient.Amount;
            public ReagentFilterMaterial MaterialFilter = new();
            public ReagentFilterItem ItemFilter = new();
            public List<Func<MaterialDef, bool>> Modifiers = new();
            public List<ReagentFilter> Conditions = new();
            public static Reagent Create(string name, ItemDef itemDef, MaterialDef mat, MaterialTypeDef matType, int amount = 1)
            {
                var r = new Reagent
                {
                    Name = name,
                    MaterialFilter = new ReagentFilterMaterial(mat, matType),
                    ItemFilter = new ReagentFilterItem(itemDef),
                };
                return r;
            }
            public Reagent()
            {

            }
            public Reagent(string name, Ingredient ing)
            {
                this.Name = name;
                this.Ingredient = ing;
            }
            public Reagent(string name, params ReagentFilter[] conditions)
            {
                this.Name = name;
                foreach (var cond in conditions)
                    this.Conditions.Add(cond);
            }
            static public List<Reagent> Create(params Reagent[] reagents)
            {
                return new List<Reagent>(reagents);
            }
            internal static ReagentFilter IsOfMaterial(MaterialDef material)
            {
                return new IsOfMaterial(material);
            }
            internal static ReagentFilter IsOfMaterialType(params MaterialTypeDef[] materialTypes)
            {
                return new IsOfMaterialType(materialTypes);
            }
            internal static ReagentFilter CanProduce(Product.Types type)
            {
                return new CanProduce(type);
            }
            
            public bool Filter(GameObject entity)
            {
                return this.Ingredient.Evaluate(entity as Entity);
            }
            public bool Filter(ItemDef def)
            {
                return this.ItemFilter.Condition(def);

            }
            public bool Filter(MaterialDef mat)
            {
                return
                    this.MaterialFilter.Condition(mat) &&
                    this.Modifiers.All(mod => mod(mat));
            }

            public override string ToString()
            {
                return this.Name;
            }

            public Reagent HasReactionClass(ReactionClass type)
            {
                this.Modifiers.Add(mat => mat.Type.ReactionClass == type);
                return this;
            }

            internal Control GetGui()
            {
                //return new GroupBox().AddControlsHorizontally(new Label($"{this.Name}: "), this.Ingredient.GetGui());
                return new GroupBox().AddControlsHorizontally(UI.Label.ParseNewNew($"{this.Name}: ", this.Ingredient));
            }
        }
    }
}
