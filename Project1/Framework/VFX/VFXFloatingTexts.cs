using Project1.Framework.Attributes;
using Project1.Framework.Base;
using Project1.Framework.UI;
using Start_a_Town_;
using Start_a_Town_.UI;

namespace Project1.Framework.VFX
{
    [EnsureStaticCtorCall]
    internal static class VFXFloatingTexts
    {
        static VFXFloatingTexts()
        {
            Registry.MapEventHooksClient.Register<SkillLevelUpEvent>(OnSkillIncreased);
        }

        private static void OnSkillIncreased(SkillLevelUpEvent e)
        {
            var actor = e.Actor;
            var skill = e.Actor.Skills[e.Skill.Def];
            //FloatingText.Create(actor, $"{skill.SkillDef.Label} increased!", ft => ft.Font = UIManager.FontBold);
            FloatingText.Create(actor.Map, actor.Global, $"{skill.SkillDef.Label} increased!", ft => ft.Font = UIManager.FontBold);
        }
    }
}
