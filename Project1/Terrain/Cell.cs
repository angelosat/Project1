using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class Cell : Inspectable, ISlottable
    {
        public const int HitPointsMax = 4;

        public string GetName()
        {
            return this.Block.Label;
        }
        public Icon GetIcon()
        {
            return new Icon(this.Block.Variations.First());
        }
        public string GetCornerText()
        {
            return "";
        }
        public Color GetSlotColor()
        {
            return Color.White;
        }
        public void GetTooltipInfo(Control tooltip)
        {
            tooltip.Controls.Add(new Label(this.Block.GetName(this.BlockData)) { Location = tooltip.Controls.BottomLeft });
        }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            this.Block.DrawUI(sb, pos, this.BlockData);
        }
        
        public static void Initialize()
        {

        }
        static readonly BitVector32.Section _orientation, _variation,_luminance, _blockData, _valid, _discovered, _originx, _originy, _originz, _damage;

        static Cell()
        {
            _valid = BitVector32.CreateSection(1); //1 bits
            _discovered = BitVector32.CreateSection(1, _valid); //1 bits
            _orientation = BitVector32.CreateSection(3, _discovered); //2 bits
            _variation = BitVector32.CreateSection(3, _orientation); //2 bits
            _luminance = BitVector32.CreateSection(15, _variation); //4 bits
            _blockData = BitVector32.CreateSection(15, _luminance); //4 bits // sum: 14bits
            _originx = BitVector32.CreateSection(2, _blockData); //2 bits // sum: 16bits
            _originy = BitVector32.CreateSection(2, _originx); //2 bits // sum: 18bits
            _originz = BitVector32.CreateSection(2, _originy); //2 bits // sum: 20bits
            _damage = BitVector32.CreateSection(HitPointsMax, _originz); //3 bits // sum: 23bits

        }
        public float HitPointsPercentage => this.HitPoints / (float)HitPointsMax;
        public int HitPoints
        {
            get => HitPointsMax - this.Damage;
            set => this.Damage = HitPointsMax - value;
        }
        public int Damage
        {
            get => this.Data[_damage];
            set => this.Data[_damage] = value;
        }
        public int OriginX
        {
            get
            {
                var val = this.Data[_originx];
                return val == 3 ? -1 : val; // because 3 == 0b11
            }
            set
            {
                if (value > 1 || value < -1)
                    throw new Exception();
                this.Data[_originx] = value;// + 1;
            }
        }
        public int OriginY
        {
            get
            {
                var val = this.Data[_originy];
                return val == 3 ? -1 : val; // because 3 == 0b11
            }
            set
            {
                if (value > 1 || value < -1)
                    throw new Exception();
                this.Data[_originy] = value;// + 1;
            }
        }
        public int OriginZ
        {
            get
            {
                var val = this.Data[_originz];
                return val == 3 ? -1 : val; // because 3 == 0b11
            }
            set
            {
                if (value > 1 || value < -1)
                    throw new Exception();
                this.Data[_originz] = value;// + 1;
            }
        }
        public IntVec3 Origin
        {
            get => new(this.OriginX, this.OriginY, this.OriginZ);
            set
            {
                this.OriginX = value.X;
                this.OriginY = value.Y;
                this.OriginZ = value.Z;
            }
        }

        public static IntVec3 GetOrigin(MapBase map, IntVec3 current)
        {
            var source = map.GetCell(current).Origin;
            var handled = new HashSet<IntVec3>() { current };
            while (source != IntVec3.Zero)
            {
                current += source;
                if (handled.Contains(current))
                    throw new Exception("loop detected in cell sources");
                handled.Add(current);
                source = map.GetCell(current).Origin;
            }
            return current;
        }
        public static IEnumerable<IntVec3> GetBlockStructure(MapBase map, IntVec3 current)
        {
            var origin = GetOrigin(map, current);
            var origincell = map.GetCell(origin);
            var originblock = origincell.Block;
            var parts = originblock.GetChildren(origincell.Orientation);
            return parts.Select(p => origin + p);
        }
        public IEnumerable<IntVec3> GetParts(IntVec3 global)
        {
            return this.Block.GetChildren(this.Orientation).Select(v => v + global);
        }

        public byte X; // 1 byte
        public byte Y; // 1 byte
        public byte Z; // 1 byte
        public Block Block = BlockDefOf.Air; // 4 bytes
        public MaterialDef Material = MaterialDefOf.Air;
        public BitVector32 Data; // 4 bytes

        public Cell()
        {
            this.Valid = true;
        }
        public Cell(int localX, int localY, int localZ) : this()
        {
            this.X = (byte)localX;
            this.Y = (byte)localY;
            this.Z = (byte)localZ;
        }

        public static IntVec3 FrontDefault = new(0, 1, 0);
        public IntVec3 Front => Coords.Rotate(FrontDefault, this.Orientation);// GetFront(this.Orientation);
        public IntVec3 Back => -this.Front;
        public override string Label => this.Block.Label;
        public bool Valid
        {
            get => this.Data[_valid] == 1;
            set => this.Data[_valid] = value ? 1 : 0;
        }
        public bool Discovered
        {
            get => this.Data[_discovered] == 1;
            set => this.Data[_discovered] = value ? 1 : 0;
        }
        public byte BlockData
        {
            get => (byte)this.Data[_blockData];
            set => this.Data[_blockData] = value;
        }
        public bool Opaque => this.Block.IsOpaque(this);
        public int Orientation
        {
            get => this.Data[_orientation];
            set => this.Data[_orientation] = value;
        }
        public int Variation
        {
            get => this.Data[_variation];
            set => this.Data[_variation] = value;
        }
        public bool IsRoomBorder => this.Block.IsRoomBorder;
        public byte Luminance
        {
            get => (byte)this.Data[_luminance];
            set => this.Data[_luminance] = value;
        }
        public float Fertility => this.Block.GetFertility(this);
        public IntVec3 LocalCoords => new(this.X, this.Y, this.Z);

        public IntVec3 SizeRotated => Coords.Rotate(this.Block.Size, this.Orientation);

        public IntVec3 GetGlobalCoords(Chunk chunk)
        {
            return new IntVec3(chunk.Start.X + this.X, chunk.Start.Y + this.Y, this.Z);
        }
        public bool IsSolid()
        {
            return this.Block.IsSolid(this);
        }
        public bool IsInvisible()
        {
            return !this.Opaque;
        }
        public static bool IsInvisible(Cell cell)
        {
            return cell.IsInvisible();
        }

        public SaveTag Save()
        {
            var data = new SaveTag(SaveTag.Types.Compound);
            data.Save(this.Block, "Block");
            this.Material.Save(data, "Material");
            this.Data.Data.Save(data, "Data");
            return data;
        }
        public Cell Load(SaveTag data)
        {
            this.Block = data.LoadBlock("Block");
            this.Data = new BitVector32((int)data["Data"].Value);
            this.Material = data.LoadDef<MaterialDef>("Material");
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Block);
            this.Material.Write(w);
            w.Write(this.Data.Data);

        }
        public Cell Read(BinaryReader r)
        {
            this.Block = r.ReadBlock();
            this.Material = Def.GetDef<MaterialDef>(r);
            this.Data = new BitVector32(r.ReadInt32());
            return this;
        }
       
        internal IEnumerable<IntVec3> GetInteractionSpotLocal()
        {
            return this.Block.GetInteractionSpotsLocal(this.Orientation);
        }
        internal IEnumerable<IntVec3> GetInteractionSpots(IntVec3 global)
        {
            return this.Block.GetInteractionSpots(this, global);
        }
        public static IEnumerable<IntVec3> GetFreeInteractionSpots(MapBase map, IntVec3 global, Actor actor)
        {
            var interactionSpots = map.GetCell(global).GetInteractionSpots(global);
            return interactionSpots.Where(s => actor.CanStandInNew(s));
        }

        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            info.AddInfo(new BarNew(() => HitPointsMax, () => this.HitPoints) { Color = Color.CornflowerBlue, Format = "Hit Points: {0} / {1}" });
            map.GetBlockEntity(vector3)?.GetSelectionInfo(info, map, vector3);
        }
        internal virtual void GetSelectionInfo(SelectionManagerNew info, MapBase map, IntVec3 vector3)
        {
            info.AddInfo(new BarNew(() => HitPointsMax, () => this.HitPoints) { Color = Color.CornflowerBlue, Format = "Hit Points: {0} / {1}" });
            map.GetBlockEntity(vector3)?.GetSelectionInfo(info, map, vector3);
        }
        internal float GetBlockHeight(Vector3 vec3)
        {
            var offset = vec3.ToBlock();
            return this.Block.GetHeight(this.BlockData, offset.X, offset.Y);
        }
    }
}
