using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Quests;

internal sealed class BlockQuestsComp : BlockComp
{
    public new class Spec : BlockComp.Spec
    {
        public override Type CompType => typeof(BlockQuestsComp);

        public override BlockQuestsComp CreateComp() => new();
    }
    public override BlockCompDef CompDef => BlockCompDefOf.Quests;

    readonly ChangeNotifier Notifier = new();
    internal int ReservedBudget
    {
        get => field;
        set
        {
            field = value;
            this.Notifier.Notify();
        }
    }

    internal override IEnumerable<Control> GetInspectorControls()
    {
        yield return new LabelNew(() => $"Reserved budget: {this.ReservedBudget}").InvalidateOn(this.Notifier);
    }

    protected override void SaveExtra(SaveTag tag)
    {
        tag.Save("ReservedBudget", this.ReservedBudget);
    }
    public override void Load(SaveTag tag)
    {
        if (tag.TryLoadInt("ReservedBudget", out var value)) this.ReservedBudget = value;
    }
    public override void Write(IDataWriter w)
    {
        w.Write(this.ReservedBudget);
    }
    public override ISerializable Read(IDataReader r)
    {
        this.ReservedBudget = r.ReadInt32();
        return this;
    }
}
