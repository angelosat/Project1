using Project1.Core.Materials;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Tools;
using Project1.Framework.Serialization;
using Project1.Framework;

namespace Project1.Core.Legacy.Crafting.Blocks
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
            this.Block = r.ReadDef<BlockDef>().Worker;
            this.Data = r.ReadByte();
            if(r.ReadBoolean()) // has requirement
                this.Requirement = new ItemMaterialAmount(r);
        }

        public ProductMaterialPair(SaveTag tag)
        {
            this.Block = tag.LoadDef<BlockDef>("Product").Worker;
            this.Data = tag.TagValueOrDefault<byte>("Data", 0);
            tag.TryGetTag("Requirement", t => this.Requirement = new ItemMaterialAmount(t));
        }

        internal MaterialDef Material => this.Requirement?.Material;

        public override string ToString() => $"Type: {this.Block.LabelReadable}\nData: {this.Data}";

        public override string LabelReadable => this.Requirement.LabelReadable;
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
            save.SaveDef("Product", this.Block.BlockDef);
            this.Data.Save(save, "Data");
            if(this.HasReq)
                this.Requirement.Save(save, "Requirement");
            tag.Add(save);
        }
        bool HasReq => this.Requirement is not null;
        public void Write(IDataWriter w)
        {
            w.Write(this.Block.BlockDef);
            w.Write(this.Data);
            w.Write(this.HasReq);
            if (this.HasReq)
                this.Requirement.Write(w);
        }
    }
}
