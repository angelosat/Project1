using Project1.Core.Entities;
using Project1.Framework;

namespace Project1.Core.Systems.Plants
{
    public sealed class Plant : Entity
    {
        private PlantComponent _plantComponent;
        [InspectorHidden]
        public PlantComponent PlantComponent => this._plantComponent ??= this.GetComponent<PlantComponent>();
      
        public Plant() : base()
        {
        }
        public Plant(ItemDef def, int amount) : base(def, amount)
        {
        }
        
        public bool IsHarvestable => this.PlantComponent.IsHarvestable;
        [InspectorHidden]
        public float GrowthBody
        {
            set => this.PlantComponent.SetBodyGrowth(value);
        }
        [InspectorHidden]
        public float GrowthFruit
        {
            set => this.PlantComponent.SetFruitGrowth(value);
        }
    }
}
