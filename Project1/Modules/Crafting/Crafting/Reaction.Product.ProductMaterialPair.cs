using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    partial class Reaction
    {
        public partial class Product
        {
            public class ProductMaterialPair : ISaveable, ISerializableNew
            {
                public GameObject Product;
                public Reaction Reaction;
                public Dictionary<string, ObjectAmount> RequirementsNew = new();

                public ProductMaterialPair(Reaction reaction, GameObject product, Dictionary<string, ObjectAmount> ingredients)
                {
                    this.Reaction = reaction;
                    this.Product = product;
                    this.RequirementsNew = ingredients;
                }

                public ProductMaterialPair(SaveTag saveTag)
                {
                    this.Load(saveTag);
                }
                public ProductMaterialPair(IDataReader r)
                {
                    this.Read(r);
                }
                public void ConsumeMaterials()
                {
                    foreach (var req in this.RequirementsNew)
                    {
                        var reagentName = req.Key;
                        if (this.Reaction.GetIngredient(reagentName).IsPreserved)
                            continue;
                        var item = req.Value;
                        if (item.Amount > item.Object.StackSize)
                            throw new Exception("required amount is larger than actual amount");
                        item.Object.StackSize -= item.Amount;
                    }
                    if (this.RequirementsNew.Values.Any(o => o.Object.StackSize > 0))
                    {
                        // UNDONE reactions can preserve ingredients (like repairing)
                        //throw new Exception("leftover materials in reaction"); 
                    }
                }
              
                public int WorkAmount => this.Reaction.GetWorkAmount(this.RequirementsNew);

                public SaveTag Save(string name = "")
                {
                    var tag = new SaveTag(SaveTag.Types.Compound, name);
                    tag.Add(this.Product.Save("Product"));
                    this.Reaction.Save(tag, "Reaction");
                    this.RequirementsNew.SaveZip(tag, "Materials");
                    return tag;
                }

                public ISaveable Load(SaveTag tag)
                {
                    this.Product = tag.LoadObject("Product");
                    this.Reaction = tag.LoadDef<Reaction>("Reaction");
                    this.RequirementsNew.LoadZip(tag["Materials"]);
                    return this;
                }
                
                public void Write(IDataWriter w)
                {
                    this.Product.Write(w);
                    this.Reaction.Write(w);
                    this.RequirementsNew.WriteNew(w, k => w.Write(k), v => v.Write(w));
                }
                public ISerializableNew Read(IDataReader r)
                {
                    this.Product = GameObject.Create(r);
                    this.Reaction = r.ReadDef<Reaction>();
                    this.RequirementsNew.ReadNew(r, r => r.ReadString(), r => new ObjectAmount().Read(r) as ObjectAmount);
                    return this;
                }

                public static ISerializableNew Create(IDataReader r)
                {
                    return new ProductMaterialPair(r);
                }
            }
        }
    }
}
