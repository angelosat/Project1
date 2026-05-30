using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Resources;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.Equipment;

class EquipmentComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Equip;
    public override string Name { get; } = "Equippable";
      
    public GearTypeDef Type;
    public ResourceRuntime Durability;
    internal int Armor;

    public EquipmentComp()
    {
        this.Type = null;
        this.Durability = new ResourceRuntime(ResourceDefOf.Durability);
    }

    //internal override void CopyFrom(EntityComp source)
    //{
    //    var comp = (EquipmentComp)source;
    //    this.Armor = comp.Armor;
    //}

    public EquipmentComp Initialize(GearTypeDef slot)
    {
        this.Type = slot;
        return this;
    }
    public override void OnTooltipCreated(Control tooltip)
    {
        tooltip.Controls.Add(new Label(this.Durability.ToString())
        {
            Location = tooltip.Controls.BottomLeft,
            Font = UIManager.FontBold,
            TextColorFunc = () =>
            {
                if (this.Durability.Percentage > 0.5)
                    return Color.Lerp(Color.Yellow, Color.Lime, (this.Durability.Percentage - 0.5f) * 2);
                else
                    return Color.Lerp(Color.Red, Color.Yellow, this.Durability.Percentage * 2);
            }
        });
    }
    
    internal override void Validate()
    {
        EquipmentSystem.Validate(this);
    }

    internal override IEnumerable<Control> GetTooltipControls()
    {
        yield return new LabelNew(() => $"Armor: {this.Armor}");
    }

    public override void Write(IDataWriter w)
    {
        w.Write(this.Armor);
    }

    public override void Read(IDataReader r)
    {
        this.Armor = r.ReadInt32();
    }
}
