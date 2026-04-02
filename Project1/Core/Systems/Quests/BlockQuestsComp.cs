using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Project1.Core.Systems.Quests;

internal sealed class BlockQuestsComp : BlockComp
{
    public new class Spec : BlockComp.Spec
    {
        public override Type CompType => typeof(BlockQuestsComp);

        public override BlockQuestsComp CreateComp() => new();
    }
    public override BlockCompDef CompDef => BlockCompDefOf.Quests;

    int Budget;

    
}
