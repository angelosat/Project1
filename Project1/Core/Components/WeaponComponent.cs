using Project1.Core.Entities;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Components
{
    [Obsolete]
    class WeaponComponent : EntityComp
    {
        public override string Name { get; } = "Weapon";

        public float Speed;

        public WeaponComponent()
        {
            this.Speed = 1;
        }
        WeaponComponent(float speed)
        {
            this.Speed = speed;
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Speed: " + this.Speed) { Font = UIManager.FontBold });
        }
        public float GetTotalDamage()
        {
            float dmg = 0;
            return dmg;
        }
        static public float GetTotalDamage(GameObject obj)
        {
            return obj.GetComponent<WeaponComponent>().GetTotalDamage();
        }
    }
}
