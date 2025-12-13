using Start_a_Town_.UI;
using System;

namespace Start_a_Town_.Components
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

        public override object Clone()
        {
            return new WeaponComponent(this.Speed);
        }

        public float GetTotalDamage()
        {
            float dmg = 0;
            //foreach(var stat in this.Damage)
            //    dmg += stat.Value;
            return dmg;
        }
        static public float GetTotalDamage(GameObject obj)
        {
            return obj.GetComponent<WeaponComponent>().GetTotalDamage();
        }
    }
}
