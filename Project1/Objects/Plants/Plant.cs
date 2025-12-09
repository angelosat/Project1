using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public sealed class Plant : Entity
    {
        private PlantComponent _plantComponent;
        [InspectorHidden]
        public PlantComponent PlantComponent => this._plantComponent ??= this.GetComponent<PlantComponent>();
      
        public override GameObject Create()
        {
            return new Plant();
        }
        public Plant() : base()
        {
            this.AddComponent(new PlantComponent());
            this.AddComponent(new ResourcesComponent(ResourceDefOf.HitPoints));
        }

        public Plant(ItemDef def)
            : base()
        {
            this.Def = def;
            this.AddComponent(new SpriteComponent(Def));
        }
       
        public bool IsHarvestable => this.PlantComponent.IsHarvestable;
        [InspectorHidden]
        public float GrowthBody
        {
            //get => this.PlantComponent.GrowthBody.Percentage;
            set => this.PlantComponent.SetBodyGrowth(value);
        }
        [InspectorHidden]
        public float GrowthFruit
        {
            //get => this.PlantComponent.FruitGrowth.Percentage;
            set => this.PlantComponent.SetFruitGrowth(value);
        }
    }
}
