using System.IO;

namespace Start_a_Town_.Components.Crafting
{
    public class ProductMaterialPair : Inspectable
    {
        public Block Block;

        public byte Data;

        public int Orientation;

        public ToolUseDef Skill;

        public ItemMaterialAmount Requirement;

        public ProductMaterialPair(Block block, ItemMaterialAmount itemMaterial)
        {
            this.Block = block;
            this.Requirement = itemMaterial;
        }

        public ProductMaterialPair(IDataReader r)
        {
            //this.Block = r.ReadBlock();
            this.Block = Block.GetBlock(r.ReadInt32());
            this.Data = r.ReadByte();
            if(r.ReadBoolean()) // has requirement
                this.Requirement = new ItemMaterialAmount(r);
        }

        public ProductMaterialPair(SaveTag tag)
        {
            this.Block = tag.LoadBlock("Product");
            this.Data = tag.TagValueOrDefault<byte>("Data", 0);
            //this.Requirement = new ItemMaterialAmount(tag["Requirement"]);
            tag.TryGetTag("Requirement", t => this.Requirement = new ItemMaterialAmount(t));
        }

        internal MaterialDef Material => this.Requirement?.Material;

        public override string ToString() => $"Type: {this.Block.Label}\nData: {this.Data}";

        //public override string Label => $"{this.Requirement.Material.Label} {this.Requirement.Item.Label} {0} / {this.Requirement.Amount}";
        public override string Label => this.Requirement.Label;// $"{this.Requirement.Amount}x {this.Requirement.Material.Label} {this.Requirement.Item.Label}";

        public string GetName() => this.Requirement.ToString();

        public ToolUseDef GetSkill()
        {
            return this.Skill;
        }
        public void Place(MapBase map, IntVec3 global)
        {
            var block = this.Block;
            var ori = this.Orientation;
            var mat = this.Material ?? MaterialDefOf.Air;
            Block.Place(block, map, global, mat, this.Data, 0, ori, true);
        }

        internal void Save(SaveTag tag, string name)
        {
            var save = new SaveTag(SaveTag.Types.Compound, name);
            save.Save(this.Block, "Product");
            this.Data.Save(save, "Data");
            if(this.HasReq)
                this.Requirement.Save(save, "Requirement");
            tag.Add(save);
        }
        bool HasReq => this.Requirement is not null;
        public void Write(BinaryWriter w)
        {
            w.Write(this.Block);
            w.Write(this.Data);
            w.Write(this.HasReq);
            if (this.HasReq)
                this.Requirement.Write(w);
        }
    }
}
