using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class EquipComponent : EntityComponent
    {
        public override string Name { get; } = "Equippable";
          
        public GearType Type;
        public Resource Durability;
        public EquipComponent()
        {
            this.Type = null;
            this.Durability = new Resource(ResourceDefOf.Durability);
        }

        public EquipComponent Initialize(GearType slot)
        {
            this.Type = slot;
            return this;
        }

        public override object Clone()
        {
            throw new Exception();
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
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

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new Equip());
            actions.Add(new EquipFromInventory());
        }
      
        internal override void SaveExtra(SaveTag tag)
        {
            base.SaveExtra(tag);
        }
        internal override void LoadExtra(SaveTag save)
        {
        }

        public override void Write(System.IO.BinaryWriter io)
        {
        }
        public override void Read(System.IO.BinaryReader io)
        {
        }
    }
}
