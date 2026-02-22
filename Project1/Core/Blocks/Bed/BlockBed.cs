using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Materials;
using Project1.Core.Loot;
using Project1.Core.Legacy;
using Project1.Core.Towns;
using Project1.Core.Helpers;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Rooms;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using Project1.Framework.Graphics;
using Project1.Framework;
using Project1.Core.Graphics;
using Project1.Core.Construction;

namespace Project1.Core
{
    class BlockBed : Block
    {
        public enum Part { Top = 0x0, Bottom = 0x1 }

        readonly AtlasDepthNormals.Node.Token[] TopParts, BottomParts;
        readonly AtlasDepthNormals.Node.Token[][] Parts;
        public BlockBed() 
            : base("Bed", 0f, 1f, false, true)
        {
            this.HidingAdjacent = false;
            this.Furniture = FurnitureDefOf.Bed;
            this.BuildProperties.ToolSensitivity = 1;
            this.BuildProperties.Dimension = 3;
            this.Ingredient = new Ingredient().IsBuildingMaterial();
            this.BuildProperties.Complexity = 10;
            this.BuildProperties.Category = ConstructionCategoryDefOf.Furniture;
            this.TopParts = [
                Atlas.Load("blocks/bed/bedslimtop2", "blocks/bed/bedslimtop2depth", "blocks/bed/bedslimtop2normal"),
                Atlas.Load("blocks/bed/bedslimbottom", "blocks/bed/bedslimbottomdepth", "blocks/bed/bedslimbottomnormal"),
                Atlas.Load("blocks/bed/bedslimbottom2", "blocks/bed/bedslimbottom2depth", "blocks/bed/bedslimbottom2normal"),
                Atlas.Load("blocks/bed/bedslimtop", "blocks/bed/bedslimtopdepth", "blocks/bed/bedslimtopnormal")
            ];
            this.BottomParts = [
                Atlas.Load("blocks/bed/bedslimbottom2", "blocks/bed/bedslimbottom2depth", "blocks/bed/bedslimbottom2normal"),
                Atlas.Load("blocks/bed/bedslimtop", "blocks/bed/bedslimtopdepth", "blocks/bed/bedslimtopnormal"),
                Atlas.Load("blocks/bed/bedslimtop2", "blocks/bed/bedslimtop2depth", "blocks/bed/bedslimtop2normal"),
                Atlas.Load("blocks/bed/bedslimbottom", "blocks/bed/bedslimbottomdepth", "blocks/bed/bedslimbottomnormal")
            ];

            this.Variations.Add(this.BottomParts.First());

            this.Parts = new AtlasDepthNormals.Node.Token[2][];
            this.Parts[0] = this.TopParts;
            this.Parts[1] = this.BottomParts;
            this.UtilitiesProvided.Add(Utility.Types.Sleeping);
            this.Size = new(1, 2, 1);
        }
        public override bool IsRoomBorder => false;
        public override bool IsStandableOn => false;
        public override float GetHeight(byte data, float x, float y)
        {
            return 0.5f;
        }

        public override bool Multi => true;
            
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new LootWrapper(a => ItemFactory.CreateFrom(RawMaterialDefOf.Planks, MaterialDefOf.Human))// this.GetMaterial(data)))
                    );
            return table;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            GetState(data, out var part, out var ori);
            var token = this.Parts[(int)part][(ori + cameraRotation) % 4];
            return token;
        }
        public static void GetState(byte data, out Part part, out int orientation)
        {
            part = (Part)(data & 0x1);
            orientation = (data & 0x6) >> 1;
        }
        public static int GetOrientation(byte data)
        {
            return (data & 0x6) >> 1;
        }
        public static void GetState(Cell cell, out Part part, out int orientation)
        {
            GetState(cell.BlockData, out part, out orientation);
        }
        public static void GetState(MapBase map, Vector3 global, out Part part, out int orientation)
        {
            GetState(map.GetCell(global), out part, out orientation);
        }
        public static byte GetData(Part part, int orientation)
        {
            byte data = 0;
            data = (byte)(data | (byte)part);
            data = (byte)(data | ((byte)orientation << 1));
            return data;
        }

        public override FurnitureDef GetFurnitureRole(MapBase map, IntVec3 global)
        {
            return IsTop(map, global) ? this.Furniture : null;
        }

        private static bool IsTop(MapBase map, IntVec3 global)
        {
            return map.GetBlockEntity(global) is BlockBedEntity;
        }

        internal override IEnumerable<(IntVec3 global, byte data)> GetFootprint(MapBase map, IntVec3 global, int orientation)
        {
            var top = global;
            var bottom = global + Coords.Rotate(IntVec3.UnitY, orientation);
            yield return (top, 0);
            yield return (bottom, 0);
        }
        [Obsolete]
        internal override void OnPlaced(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            if (!IsValidPosition(map, global, orientation))
                return;

            var top = global;
            var bottom = global + Coords.Rotate(IntVec3.UnitY, orientation);
            
            map.SetBlock(bottom, this, material, 0, 0, orientation, notify);
            map.SetBlock(top, this, material, 0, 0, orientation, notify);

            var entity = new BlockBedEntity(this.BlockDef, global);
            map.AddBlockEntity(entity);
            map.Town.AddUtility(Utility.Types.Sleeping, top);
        }
        
        public override bool IsValidPosition(MapBase map, IntVec3 global, int orientation)
        {
            var positions = new List<IntVec3> { global };

            positions.Add(orientation switch
            {
                0 => global + IntVec3.UnitX,
                1 => global + IntVec3.UnitY,
                2 => global - IntVec3.UnitX,
                3 => global - IntVec3.UnitY,
                _ => throw new Exception()
            });
            if (positions.Any(pos => map.GetBlock(pos) != BlockDefOf.Air.Block))
                return false;
            return true;
        }
        public override AtlasDepthNormals.Node.Token GetToken(int cameraRotation, Cell cell)
        {
            var origin = cell.Origin;
            var part = origin == IntVec3.Zero ? Part.Top : Part.Bottom;
            return this.Parts[(int)part][(cell.Orientation + cameraRotation) % 4];
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, IntVec3 global, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
        {
            var map = chunk.Map;
            var origin = Cell.GetOrigin(map, global);// map.GetCell(global)
            var part = origin == global ? Part.Top : Part.Bottom;
            var token = this.Parts[(int)part][(orientation + (int)camera.Rotation) % 4];

            var comp = chunk.Map.GetBlockEntityComp<BlockBedComp>(origin);
            var col = comp.GetColorFromType();

            return canvas.NonOpaque.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, col /*Color.White*/, sunlight, blocklight, depth, this, global);
        }
        public override void DrawPreview(MySpriteBatch sb, MapBase map, IntVec3 global, Camera cam, Color tint, byte data, MaterialDef material, int variation = 0, int orientation = 0)
        {
            var top = global;
            var bottom = global + Coords.Rotate(IntVec3.UnitY, orientation);
            var bottomSecIndex = (int)cam.Rotation;
            var topSrcIndex = (int)cam.Rotation;

            switch (orientation)
            {
                case 1:
                    bottomSecIndex += 1;
                    topSrcIndex += 1;
                    break;

                case 2:
                    bottomSecIndex += 2;
                    topSrcIndex += 2;
                    break;

                case 3:
                    bottomSecIndex += 3;
                    topSrcIndex += 3;
                    break;

                default: break;
            }
            bottomSecIndex %= 4;
            topSrcIndex %= 4;
            var topSrc = this.Parts[0][topSrcIndex];
            var bottomSrc = this.Parts[1][bottomSecIndex];

            var topd = top.GetDrawDepth(map, cam);
            var bottomd = bottom.GetDrawDepth(map, cam);
            if (topd > bottomd)
            {
                sb.DrawBlock(Atlas.Texture, map, top, topSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
                sb.DrawBlock(Atlas.Texture, map, bottom, bottomSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
            }
            else
            {
                sb.DrawBlock(Atlas.Texture, map, bottom, bottomSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
                sb.DrawBlock(Atlas.Texture, map, top, topSrc, cam, Color.Transparent, tint, Color.White, Vector4.One);
            }
        }
        public override BlockEntity GetBlockEntityOrNew(MapBase map, IntVec3 originGlobal, BlockComp.Spec args)
        {
            return new BlockBedEntity(this.BlockDef, originGlobal);
        }
       
        public static BlockBedEntity GetEntity(MapBase map, IntVec3 global)
        {
            return map.GetBlockEntity<BlockBedEntity>(Cell.GetOrigin(map, global));
        }
       
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var entity = GetEntity(map, vector3);
            entity.GetSelectionInfo(info, map, vector3);
        }

        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal()
        {
            yield return new IntVec3(-1, 0, 0);
        }
        protected override IEnumerable<IntVec3> GetInteractionSpotsLocal(MapBase map, IntVec3 global)//int orientation)
        {
            yield return -IntVec3.UnitX;
        }
    }
}
