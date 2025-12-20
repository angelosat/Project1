using System;

namespace Start_a_Town_
{
    public class BlockDef : Def
    {
        public Type BlockType;
        public Type[] BlockEntityComps;
        //public BlockDef()
        //{

        //}
        public BlockDef(string name, Type blockType, Type[] entityComps = null) : base(name)
        {

        }
    }
}
