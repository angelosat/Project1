using Project1.Core.Blocks;
using Project1.Core.Materials;
using Project1.Framework;

namespace Project1.Core.Simulation
{

    public readonly struct CellQuery
    {
        private readonly Chunk _chunk;
        private readonly CellId _cellIndex;

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
        internal CellQuery(Chunk chunk, CellId cellIndex)
        {
            this._cellIndex = cellIndex;
            this._chunk = chunk;
        }
        public byte BlockData
        {
            get => this._chunk.GetBlockData(_cellIndex);
            set
            {
                this._chunk.SetBlockData(_cellIndex, value);
                this.Map.Events.Post(new CellEditEvent(this));
            }
        }

        public int Data
        {
            get => this._chunk.GetData(_cellIndex);
            set
            {
                this._chunk.SetData(_cellIndex, value);
                this.Map.Events.Post(new CellEditEvent(this));
            }
        }

        public Block Block
        {
            get => this._chunk.GetBlock(_cellIndex);
            set
            {
                this._chunk.SetBlock(_cellIndex, value);
                this.Map.Events.Post(new CellEditEvent(this));
            }
        }

        public MaterialDef Material
        {
            get => this._chunk.GetMaterial(_cellIndex);
            set
            {
                this._chunk.SetMaterial(_cellIndex, value);
                this.Map.Events.Post(new CellEditEvent(this));
            }
        }

        public int Variation
        {
            get => this._chunk.GetVariation(_cellIndex);
            set
            {
                this._chunk.SetVariation(_cellIndex, value);
                this.Map.Events.Post(new CellEditEvent(this));
            }
        }
    }
}
