using Microsoft.Xna.Framework;
using Project1.Core.Inventory;
using Project1.Core.Resources;
using Project1.Core.Skills;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.UI;

namespace Project1.Core.VFX
{
    [EnsureStaticCtorCall]
    internal static class VFXFloatingTexts
    {
        static VFXFloatingTexts()
        {
            Registry.MapEventHooksClient.Register<SkillLevelUpEvent>(OnSkillIncreased);
            Registry.MapEventHooksClient.Register<InventoryItemAddedEvent>(OnItemGot);
            Registry.MapEventHooksClient.Register<InventoryItemRemovedEvent>(OnItemLost);
            Registry.MapEventHooksClient.Register<ResourceAdjustedEvent>(OnHealthLost);
        }
        private static void OnSkillIncreased(SkillLevelUpEvent e)
        {
            var actor = e.Actor;
            var skill = e.Actor.Skills[e.Skill.Def];
            FloatingText.Create(actor.Map, actor.Global, $"{skill.SkillDef.LabelReadable} increased!", ft => ft.Font = UIManager.FontBold);
        }

        private static void OnItemGot(InventoryItemAddedEvent e)
        {
            var parent = e.Actor;
            var item = e.Item;
            var floating = new FloatingTextEx(parent,
                new FloatingTextEx.Segment("Received ", Color.Lime),
                new FloatingTextEx.Segment(item.Name, item.GetInfo().GetQualityColor())
                );
            floating.Show();
        }
        private static void OnItemLost(InventoryItemRemovedEvent e)
        {
            var parent = e.Actor;
            var item = e.Item; 
            FloatingTextEx floating = new FloatingTextEx(parent,
                //new FloatingTextEx.Segment("Lost " + amount.ToString() + "x ", Color.Red),
                new FloatingTextEx.Segment(item.Name, item.GetInfo().GetQualityColor())
                );
            floating.Show();
        }
        private static void OnHealthLost(ResourceAdjustedEvent e)
        {
            if (e.Def != ResourceDefOf.Health)
                return;
            var dmg = e.Value;
            var recipient = e.Owner;
            var floating = new FloatingText(recipient, dmg.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };
            floating.Show();
        }
    }
}
