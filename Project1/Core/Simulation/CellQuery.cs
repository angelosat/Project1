using Project1.Core.Blocks;
using Project1.Core.Systems.Materials;
using Project1.Framework;

namespace Project1.Core.Simulation
{
    
    public readonly struct CellQuery
    {
        private readonly Chunk _chunk;
        private readonly CellId _cellIndex;
        public readonly IntVec3 GetGlobal() => this.GetLocal().ToGlobal(this._chunk);
        public readonly IntVec3Local GetLocal() => Chunk.GetLocalFromIndex(this._cellIndex);
        //public readonly IntVec3Local Local;
        public readonly bool Exists => this._chunk is not null;
        //public readonly Cell Cell;// => this._chunk.GetLocalCell(this._cellIndex);

        public MapBase Map => this._chunk.Map;
     
        internal CellQuery(Chunk chunk, IntVec3Local pos)
        {
            this._chunk = chunk;
            this._cellIndex = Chunk.GetCellIndex(pos);
            //this.Local = pos;
            //this.Global = pos.ToGlobal(chunk);
        }

        internal CellQuery(MapBase map, IntVec3 global)
        {
            this._chunk = map.GetChunk(global);
            this._cellIndex = Chunk.GetCellIndex(global);
            //this.Global = global;
            //this.Local = global.ToLocal();
        }
        internal CellQuery(Chunk chunk, CellId cellIndex)
        {
            this._cellIndex = cellIndex;
            this._chunk = chunk;
            //this.Local = Chunk.GetLocalFromIndex(cellIndex);
            //this.Global = this.Local.ToGlobal(chunk);
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
