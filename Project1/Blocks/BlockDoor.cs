using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class BlockDoor : Block
    {
        const byte MaskOpen = 0b1, MaskLocked = 0b10;
        static readonly AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        public BlockDoor()
            : base("Door", 0, 1, true, true)
        {
            this.HidingAdjacent = false;
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

            var ndepth = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorndepth");
            var wdepth = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorwdepth");
            var nnormals = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doornnormals");
            var wnormals = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorwnormals");

            Orientations[0] = Atlas.Load("blocks/doors/doors", BlockDepthMap, NormalMap);
            Orientations[1] = Atlas.Load("blocks/doors/doore", BlockDepthMap, NormalMap);
            Orientations[2] = Atlas.Load("blocks/doors/doorn", ndepth, nnormals);
            Orientations[3] = Atlas.Load("blocks/doors/doorw", wdepth, wnormals);
            this.BuildProperties.Category = ConstructionCategoryDefOf.Doors;
            this.BuildProperties.Dimension = 8;
            this.Size = new(1, 1, 2);
        }

        public override bool IsDeconstructible => true;
        public override bool IsRoomBorder => true;
        public override bool Multi => true;

        public static byte GetData(bool open, bool locked)
        {
            byte val = 0;
            if (open)
                val |= MaskOpen;
            if (locked)
                val |= MaskLocked;
            return val;
        }
        public static (bool open, bool locked) GetState(byte data)
        {
            return ((data & MaskOpen) == MaskOpen, (data & MaskLocked) == MaskLocked);
        }

        public static void Read(byte data, out bool locked, out bool open, out int part)
        {
            locked = IsLocked(data);
            open = IsOpen(data);
            part = GetPart(data);
        }
        public static bool IsLocked(byte data)
        {
            return GetState(data).locked;
        }
        public static bool IsOpen(byte data)
        {
            return GetState(data).open;
        }
        /// <summary>
        /// returns z-distance from bottom block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// 
        public static int GetPart(byte data)
        {
            return data & 0x3;
        }

        public override bool IsOpaque(Cell cell)
        {
            return !IsOpen(cell.BlockData);
        }

        internal override bool IsPathable(Cell cell, IntVec3 blockCoords)
        {
            return !IsLocked(cell.BlockData);
        }
        protected override void Place(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            var positions = new IntVec3[2];
            for (int i = 0; i < 2; i++)
            {
                var g = global + new IntVec3(0, 0, i);
                positions[i] = g;
                byte _data = 0;// (byte)i;
                map.SetBlock(g, this, material, _data, variation, orientation, false);
            }
            if (notify)
                map.NotifyBlocksChanged(positions);

            // DETECT HOUSE
            // find which side is enterior by checking heightmap
            var back = global - IntVec3.UnitY;
            var front = global + IntVec3.UnitY;
            var backHeightMap = map.GetHeightmapValue(back);
            var frontHeightMap = map.GetHeightmapValue(front);
            var backIsInside = back.Z < backHeightMap;
            var frontIsInside = front.Z < frontHeightMap;
            if (backIsInside)
            {
                //flood fill to find all enterior
            }
        }

        private static IntVec3 GetCenter(byte data)
        {
            byte masked = data &= 0x1;
            int baseZ = -masked;
            var baseLoc = new IntVec3(0, 0, baseZ);
            return baseLoc;
        }

        public override bool IsSolid(Cell cell)
        {
            Read(cell.BlockData, out var locked, out var open, out var part);
            return !open;
        }
        public override bool IsSolid(Cell cell, Vector3 withinBlock)
        {
            return this.IsSolid(cell);
        }
        public override float GetDensity(byte data, Vector3 global)
        {
            Read(data, out var locked, out var open, out var part);
            return open ? 0 : 1;
        }

        public override Icon GetIcon()
        {
            return new Icon(this.GetDefault());
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(m => m.Type == MaterialTypeDefOf.Stone || m.Type == MaterialTypeDefOf.Metal || m.Type == MaterialTypeDefOf.Wood);
        }

        public override void GetTooltip(UI.Control tooltip, MapBase map, IntVec3 global, IntVec3 face)
        {
            base.GetTooltip(tooltip, map, global, face);
            var cell = map.GetCell(global);
            var data = cell.BlockData;// map.GetData(global); //
            var open = (data & 0x4) == 0x4;

            Cell cell2 = map.GetCell(global);
            bool lastOpen = (cell.BlockData & 0x4) == 0x4;
            var locked = IsLocked(cell.BlockData);
            tooltip.Controls.Add(new Label(open ? "Open" : "Closed") { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new Label(locked ? "Locked" : "Unlocked") { Location = tooltip.Controls.BottomLeft });
        }

        public override AtlasDepthNormals.Node.Token GetDefault()
        {
            return Orientations[0];
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return Orientations[orientation];
        }

        public static void Toggle(MapBase map, IntVec3 global)
        {
            var children = BlockDefOf.Door.GetChildren(map, global);
            var chunk = map.GetChunk(global);
            foreach (var g in children)
            {
                var cell = map.GetCell(g);
                if (map.GetBlock(g) is not BlockDoor)
                    throw new Exception();
                var data = cell.BlockData;
                bool lastOpen = IsOpen(data);
                var open = !lastOpen;
                if (open)
                    cell.BlockData |= MaskOpen;
                else
                    cell.BlockData ^= MaskOpen;

                //chunk.InvalidateSlice(g.Z); // this is called in invalidatecell right?
                chunk.InvalidateCell(cell); // to update light
                $"[{map.Net}] door at {g} {(open ? "opened" : "closed")}".ToConsole();
            }
        }
        protected override Rectangle ParticleTextureRect => Orientations[0].Rectangle;
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            Read(cell.BlockData, out bool locked, out bool open, out int part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Atlas.Texture, screenBounds, this.Variations[ori], zoom, fog, Color.White, sunlight, blocklight, depth, this);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            Read(cell.BlockData, out bool locked, out bool open, out int part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Atlas.Texture, screenBounds, this.Variations[ori], zoom, fog, Color.White, sunlight, blocklight, depth);
        }

        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            Read(data, out bool locked, out bool open, out int part);
            int ori = (orientation - (int)camera.Rotation + (open ? 1 : 0)); // FASTER???
            if (ori < 0)
                ori += 4;
            else
                ori %= 4;
            return canvas.Opaque.DrawBlock(Atlas.Texture, screenBounds, Orientations[ori], camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, global);
        }
        public override void DrawPreview(MySpriteBatch sb, MapBase map, IntVec3 global, Camera cam, Color tint, byte data, MaterialDef material, int variation = 0, int orientation = 0)
        {
            var token = Orientations[orientation];
            sb.DrawBlock(Atlas.Texture, map, global, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
            sb.DrawBlock(Atlas.Texture, map, global + IntVec3.UnitZ, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
            sb.DrawBlock(Atlas.Texture, map, global + IntVec3.UnitZ, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
        }
    }
}
