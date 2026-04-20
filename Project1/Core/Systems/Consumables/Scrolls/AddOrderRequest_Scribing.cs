using Project1.Core.Helpers;
using Project1.Core.Systems.Crafting;
using Project1.Core.Systems.Magic;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Consumables.Scrolls;

internal class AddOrderRequest_Scribing : AddOrderRequest
{
    internal SpellDef Spell { get; private set; }
    AddOrderRequest_Scribing() : base(WorkstationCapabilityDefOf.Scribing, ConsumableDefOf.Scroll)
    {

    }

    internal AddOrderRequest_Scribing(SpellDef spell) : this()
    {
        this.Spell = spell;
    }

    public override string GetLabel()
        => $"{ConsumableDefOf.Scroll.Verb}: {this.Spell.LabelReadable}";

    protected override void WriteExtra(IDataWriter w)
    {
        w.Write(this.Spell);
    }
    protected override void ReadExtra(IDataReader r)
    {
        this.Spell = r.ReadDef<SpellDef>();
    }

    protected override void SaveExtra(SaveTag tag)
    {
        tag.Save("Spell", this.Spell);
    }
    protected override void LoadExtra(SaveTag tag)
    {
        this.Spell = tag.LoadDef<SpellDef>("Spell");
    }
}
