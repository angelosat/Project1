using System.Xml.Serialization;

namespace Start_a_Town_
{
    public class GrowthProperties
    {
        [XmlIgnore]
        public ItemDef GrowthItemDef;
        [XmlIgnore] 
        public MaterialDef GrowthMaterial;

        [XmlElement(ElementName = "GrowthMaterialDef")]
        public string GrowthMaterialString
        {
            get => this.GrowthMaterial.Name;
            set => this.GrowthMaterial = Def.GetDef<MaterialDef>(value);
        }

        [XmlElement(ElementName = "GrowthItemDef")]
        public string GrowthItemDefString
        {
            get => this.GrowthItemDef.Name;
            set => this.GrowthItemDef = Def.GetDef<ItemDef>(value);
        }

        public int MaxYieldHarvest;
        public int GrowthLengthTicks;

        public GrowthProperties()
        {

        }
        public GrowthProperties(ItemDef growthItemDef, MaterialDef growthMaterial, int maxYieldHarvest, int growthLengthTicks)
        {
            this.GrowthItemDef = growthItemDef;
            this.GrowthMaterial = growthMaterial;
            this.MaxYieldHarvest = maxYieldHarvest;
            this.GrowthLengthTicks = growthLengthTicks * Ticks.PerSecond;
        }

        public Entity CreateEntity()
        {
            return this.GrowthItemDef.CreateFrom(this.GrowthMaterial).SetStackSize(this.MaxYieldHarvest) as Entity;
        }
    }
}
