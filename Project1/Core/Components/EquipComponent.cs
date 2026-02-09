using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Gear;
using Project1.Core.Resources;
using Project1.Core.Interactions;
using Project1.Core.UI;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework;

namespace Project1.Core.Components
{
    class EquipComponent : EntityComp
    {
        public override string Name { get; } = "Equippable";
          
        public GearTypeDef Type;
        public Resource Durability;
        public EquipComponent()
        {
            this.Type = null;
            this.Durability = new Resource(ResourceDefOf.Durability);
        }

        public EquipComponent Initialize(GearTypeDef slot)
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
            //actions.Add(new InteractionEquip());
            //actions.Add(new EquipFromInventory());
        }
      
        internal override void SaveExtra(SaveTag tag)
        {
            base.SaveExtra(tag);
        }
        internal override void LoadExtra(SaveTag save)
        {
        }

        public override void Write(IDataWriter io)
        {
        }
        public override void Read(IDataReader io)
        {
        }
    }
}
