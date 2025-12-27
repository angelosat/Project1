using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Interactions
{
    public class InteractionConstructCache : InteractionCache
    {
        public BlockConstructionComp CachedComp;
    }
    class InteractionConstructWorker : InteractionWorker
    {
        public override bool CanPerform(InteractionCache ctx)
        {
            var cache = (InteractionConstructCache)ctx;
            var target = cache.Target;
            var comp = cache.CachedComp ??= target.Map.GetBlockEntity(target.Global).Comps.GetComp<BlockConstructionComp>();
            if (comp.Map is null)
                return false;
            var manager = comp.Parent.Map.Town.ConstructionsManager;
            if (!manager.IsDesignatedConstruction(comp))
                return false;
            if (comp.Parent.CellsOccupied.Any(c => !manager.IsSupported(c)))
                return false;
            return true;
        }
        public override bool CanFinish(InteractionCache ctx)
        {
            var cache = (InteractionConstructCache)ctx;
            var target = cache.Target;
            var comp = cache.CachedComp ??= target.Map.GetBlockEntity(target.Global).Comps.GetComp<BlockConstructionComp>();
            if (comp.Parent.CellsOccupied.Any(c => comp.Parent.Map.GetEntitiesAt(c).Any()))
                return false;
            return true;
        }
    }
    class InteractionConstruct : InteractionToolUse
    {
        public InteractionConstruct()
            : base("Construct")
        {
            //this.BuildProgress = new(() => Comp.Progress); // why has this thrown null?
        }
        InteractionConstructCache _cache;
        InteractionConstructCache TypedCache => _cache ??= (InteractionConstructCache)this.Cache;
        //BlockConstructionComp _cachedComp;
        //BlockConstructionComp Comp => this._cachedComp ??= this.Target.Map.GetBlockEntity(this.Target.Global).Comps.GetComp<BlockConstructionComp>();
        protected override float Progress => this.ProgressNew.Percentage;// this.BuildProgress.Value.Percentage;
        //protected override Progress ProgressNew => this.Comp.Progress;
        protected override float WorkDifficulty { get; } = 1;

        protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnSwing;

        public override object Clone()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnApplyWork(float workAmount)
        {
            //this.BuildProgress.Value.Value += workAmount;
            this.TypedCache.CachedComp.Advance((int)workAmount);
        }

        protected override void Done()
        {
            //var a = this.Actor;
            //var t = this.Target;
            //var global = t.Global;
            //var map = a.Map;
            //var entity = t.GetBlockEntity<BlockConstructionEntity>();
            //entity.Container.Clear(); // clear materials because they get ejected when the blockconstruction remove method is called
            //var block = entity.Product.Block;
            //var cell = map.GetCell(global);
            //var ori = cell.Orientation;
            //foreach (var child in entity.Children)
            //    map.RemoveBlock(child, false);
            //Block.Place(block, map, entity.OriginGlobal, entity.Product.Material, entity.Product.Data, 0, ori, true);
            //map.GetBlockEntity(t.Global)?.IsMadeFrom(new ItemMaterialAmount[] { entity.Product.Requirement });
        }

        
        protected override Color GetParticleColor() => default;

        protected override List<Rectangle> GetParticleRects() => null;

        protected override SkillDef GetSkill() => SkillDefOf.Construction;

        protected override ToolUseDef GetToolUse() => ToolUseDefOf.Building;
    }

    //class InteractionConstruct : InteractionPerpetual
    //{
    //    Progress BuildProgress;
    //    public InteractionConstruct()
    //        : base("Construct")
    //    {
    //    }

    //    protected override void Start()
    //    {
    //        var a = this.Actor;
    //        var t = this.Target; 
    //        base.Start();
    //        var entity = a.Map.GetBlockEntity(t.Global) as IConstructible;
    //        this.BuildProgress = entity.BuildProgress;
    //        var tool = a.GetEquipmentSlot(GearType.Mainhand);
    //        var toolspeed = tool is null ? 0 : StatDefOf.ToolSpeed.GetValue(tool);
    //        var speed = 1 + toolspeed;
    //        this.Animation.Speed = speed;
    //    }
    //    bool SuccessCondition()
    //    {
    //        return this.BuildProgress.IsFinished;
    //    }
    //    public override void OnUpdate()
    //    {
    //        var a = this.Actor;
    //        var t = this.Target; 
    //        var workAmount = a.GetToolWorkAmount(ToolUseDefOf.Building);
    //        this.BuildProgress.Value += workAmount;
    //        if (SuccessCondition())
    //        {
    //            this.Done();
    //            return;
    //        }
    //    }
    //    public void Done()
    //    {
    //        var a = this.Actor;
    //        var t = this.Target;
    //        var global = t.Global;
    //        var map = a.Map;
    //        var entity = map.GetBlockEntity(global) as BlockConstructionEntity;
    //        entity.Container.Clear(); // clear materials because they get ejected when the blockconstruction remove method is called
    //        var block = entity.Product.Block;
    //        var cell = map.GetCell(global);
    //        var ori = cell.Orientation;
    //        foreach (var child in entity.Children)
    //        {
    //            map.RemoveBlock(child, false);
    //        }
    //        block.Place(map, entity.OriginGlobal, entity.Product.Material, entity.Product.Data, 0, ori, true);
    //        map.GetBlockEntity(t.Global)?.IsMadeFrom(new ItemMaterialAmount[] { entity.Product.Requirement });
    //        this.Finish();
    //    }

    //    public override object Clone()
    //    {
    //        return new InteractionConstruct();
    //    }
    //}
}
