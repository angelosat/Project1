using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class BlockDef : Def
    {
        public Type BlockType;
        public Type BlockEntityType;
        public BlockDef()
        {

        }
        public BlockDef(string name) : base(name)
        {

        }
    }
}
