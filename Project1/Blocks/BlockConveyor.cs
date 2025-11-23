using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class BlockConveyor : Block
    {
        public BlockConveyor()
            : base("Conveyor", opaque: false)
        {
            this.HidingAdjacent = false;
            var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
            this.Variations.Add(txt);
        }
        public override Vector3 GetVelocityTransform(byte data, Vector3 blockcoords)
        {
            return Vector3.UnitX * .05f;
        }
        public override float GetPathingCost(byte data)
        {
            return 0;
        }
        public override Color[] UV
        {
            get
            {
                return Block.BlockCoordinatesQuarter;
            }
        }
        public override MouseMap MouseMap
        {
            get
            {
                return Block.BlockQuarterMouseMap;
            }
        }
        public override float GetHeight(byte data, float x, float y)
        {
            return this.GetHeight(x, y);
        }
        public override float GetHeight(float x, float y)
        {
            return .25f;
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Stone;
        }
        //public override MaterialDef GetMaterial(byte blockdata)
        //{
        //    return MaterialDefOf.Stone;
        //}
    }
}
