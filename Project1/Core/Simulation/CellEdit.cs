using Project1.Core.Blocks;
using Project1.Core.Systems.Materials;
using Project1.Framework;

namespace Project1.Core.Simulation
{
    public static class CellHelpers
    {
        extension(Chunk chunk)
        {
            public CellEdit Edit(IntVec3Local pos) => new(chunk, pos);
        }
    }
    public class CellEdit
    {
        private readonly Chunk _chunk;
        private readonly int _cellIndex;

        BlockDef PendingBlock;
        MaterialDef PendingMaterial;
        int? PendingData;
        int? PendingVariation;
        byte? PendingBlockData;

        public MapBase Map => this._chunk.Map;
        public IntVec3 Global => Chunk.GetLocalFromIndex(this._cellIndex).ToGlobal(this._chunk);

        internal CellEdit(Chunk chunk, IntVec3Local pos)
        {
            this._chunk = chunk;
            this._cellIndex = Chunk.GetCellIndex(pos);
        }

        internal CellEdit(Chunk chunk, int cellIndex)
        {
            this._chunk = chunk;
            this._cellIndex = cellIndex;
        }

        internal CellEdit(MapBase map, IntVec3 global)
        {
            this._chunk = map.GetChunk(global);
            this._cellIndex = Chunk.GetCellIndex(global);
        }

        public void Flush()
        {
            this._chunk.WriteCell(this._cellIndex, this.PendingBlock, this.PendingMaterial, this.PendingVariation, this.PendingBlockData, this.PendingData);
            this.Map.Events.Post(new CellEditEvent(this.ToQuery()));
            this.Clear();
        }

        void Clear()
        {
            this.PendingBlock = null;
            this.PendingMaterial = null;
            this.PendingVariation = null;
            this.PendingBlockData = null;
            this.PendingData = null;
        }

        CellQuery ToQuery()
            => new(this._chunk, this._cellIndex);

        public byte BlockData
        {
            get => this._chunk.GetBlockData(_cellIndex);
            set => this.PendingBlockData = value;
        }

        public int Data
        {
            get => this._chunk.GetData(_cellIndex);
            set => this.PendingData = value;
        }

        public BlockDef Block
        {
            get => this._chunk.GetBlock(_cellIndex).BlockDef;
            set => this.PendingBlock = value;
        }

        public MaterialDef Material
        {
            get => this._chunk.GetMaterial(_cellIndex);
            set => this.PendingMaterial = value;
        }

        public int Variation
        {
            get => this._chunk.GetVariation(_cellIndex);
            set => this.PendingVariation = value;
        }

        public CellEdit SetBlock(BlockDef block)
        {
            this.PendingBlock = block;
            return this;
        }
        public CellEdit SetMaterial(MaterialDef material)
        {
            this.PendingMaterial = material;
            return this;
        }
        public CellEdit SetVariation(int variation)
        {
            this.PendingVariation = variation;
            return this;
        }
        public CellEdit SetBlockData(byte blockData)
        {
            this.PendingBlockData = blockData;
            return this;
        }
        public CellEdit SetData(int data)
        {
            this.PendingData = data;
            return this;
        }
    }
}
