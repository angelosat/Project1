using Project1.Framework;
using Project1.Framework.UI;
using Project1.Core.Base;
using Project1.Core.UI;
using Project1.Core.Skills;

namespace Project1.Core.VFX
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
            FloatingText.Create(actor.Map, actor.Global, $"{skill.SkillDef.LabelReadable} increased!", ft => ft.Font = UIManager.FontBold);
        }
    }
}
