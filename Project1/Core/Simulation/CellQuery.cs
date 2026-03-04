using Project1.Core.Blocks;
using Project1.Core.Materials;
using Project1.Framework;

namespace Project1.Core.Simulation
{
    public class CellEdit
    {
        private readonly Chunk _chunk;
        private readonly int _cellIndex;

        BlockDef PendingBlock;
        MaterialDef PendingMaterial;
        int PendingData;
        int PendingVariation;

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
            this._chunk.SetBlock(this._cellIndex, this.PendingBlock.Block);
            this.Map.Events.Post(new CellMutatedEvent(this.ToQuery()));
        }

        CellQuery ToQuery()
            => new(this._chunk, this._cellIndex);

        public byte BlockData
        {
            get => this._chunk.GetBlockData(_cellIndex);
            set
            {
                this._chunk.SetBlockData(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public int Data
        {
            get => this._chunk.GetData(_cellIndex);
            set
            {
                this._chunk.SetData(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public Block Block
        {
            get => this._chunk.GetBlock(_cellIndex);
            set
            {
                this._chunk.SetBlock(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public MaterialDef Material
        {
            get => this._chunk.GetMaterial(_cellIndex);
            set
            {
                this._chunk.SetMaterial(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public int Variation
        {
            get => this._chunk.GetVariation(_cellIndex);
            set
            {
                this._chunk.SetVariation(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }
    }

    public readonly struct CellQuery
    {
        private readonly Chunk _chunk;
        private readonly int _cellIndex;

        public MapBase Map => this._chunk.Map;
        public IntVec3 Global => Chunk.GetLocalFromIndex(this._cellIndex).ToGlobal(this._chunk);

        internal CellQuery(Chunk chunk, IntVec3Local pos)
        {
            this._chunk = chunk;
            this._cellIndex = Chunk.GetCellIndex(pos);
        }

        internal CellQuery(MapBase map, IntVec3 global)
        {
            this._chunk = map.GetChunk(global);
            this._cellIndex = Chunk.GetCellIndex(global);
        }

        public byte BlockData
        {
            get => this._chunk.GetBlockData(_cellIndex);
            set
            {
                this._chunk.SetBlockData(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public int Data
        {
            get => this._chunk.GetData(_cellIndex);
            set
            {
                this._chunk.SetData(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public Block Block
        {
            get => this._chunk.GetBlock(_cellIndex);
            set
            {
                this._chunk.SetBlock(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public MaterialDef Material
        {
            get => this._chunk.GetMaterial(_cellIndex);
            set
            {
                this._chunk.SetMaterial(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }

        public int Variation
        {
            get => this._chunk.GetVariation(_cellIndex);
            set
            {
                this._chunk.SetVariation(_cellIndex, value);
                this.Map.Events.Post(new CellMutatedEvent(this));
            }
        }
    }
}
