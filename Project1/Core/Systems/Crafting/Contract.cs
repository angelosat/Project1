using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Systems.Crafting;

internal record Contract(Actor Author, BlockWorkstationComp Workstation, CraftingOrder Order, IEnumerable<Entity> Ingredients)
{
    public bool IsValid => !this.Order.IsDisposed;
}
//internal sealed class CraftingCommitment(Actor actor, CraftingOrder order) : ISaveableNewNew<CraftingCommitment>
internal sealed class CraftingCommitment : ISaveableNewNew<CraftingCommitment>
{
    //internal readonly Actor Actor = actor;
    //internal readonly CraftingOrder Order = order;
    //internal Entity? Product;
    internal sealed class BoneToIngredient(BoneDef bone) : ISaveableNewNew<BoneToIngredient>
    {
        internal BoneDef Bone = bone;
        internal EntityRefId Item = EntityRefId.Null;

        public static BoneToIngredient Create(SaveTag tag)
        {
            var bone = tag.LoadDef<BoneDef>("Bone");
            var ingredient = tag.LoadId<EntityRefId>("Ingredient");
            return new BoneToIngredient(bone) { Item = ingredient };
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Save("Bone", this.Bone);
            tag.Save("Ingredient", this.Item);
            return tag;
        }
    }

    internal EntityRefId Actor { get; init; }
    internal CraftingOrderId Order { get; init; }
    internal EntityRefId? Product;
    internal SimulationTick TickCommitted { get; init; }
    internal Dictionary<BoneDef, BoneToIngredient> Ingredients { get; init; }

    CraftingCommitment()
    {
        
    }
    public CraftingCommitment(EntityRefId actor, CraftingOrderId order, SimulationTick tick, IEnumerable<BoneDef> boneLayout)
    {
        Actor = actor;
        Order = order;
        TickCommitted = tick;
        Ingredients = boneLayout.ToDictionary(b => b, b => new BoneToIngredient(b));
    }

    internal void Bind(BoneDef bone, Entity targetStack)
        => this.Ingredients[bone].Item = targetStack.RefId;

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Save("Actor", this.Actor);
        tag.Save("Order", this.Order);
        if(this.Product.HasValue)
            tag.Save("Product", this.Product.Value);
        tag.Save("Tick", this.TickCommitted);
        tag.Save("Ingredients", this.Ingredients.Values);
        return tag;
    }

    public static CraftingCommitment Create(SaveTag tag)
    {
        var actor = tag.LoadId<EntityRefId>("Actor");
        var order = tag.LoadId<CraftingOrderId>("Order");
        EntityRefId? product = tag.TryLoadId<EntityRefId>("Product", out var p) ? p : null;
        var tick = tag.LoadUlong("Tick");
        var ingredients = tag.LoadList<BoneToIngredient>("Ingredients").ToDictionary(bi => bi.Bone);
        return new CraftingCommitment() { Actor = actor, Order = order, Product = product, TickCommitted = tick, Ingredients = ingredients };
    }
}
