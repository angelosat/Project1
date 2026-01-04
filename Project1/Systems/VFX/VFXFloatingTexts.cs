using Start_a_Town_.UI;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class VFXFloatingTexts
    {
        static VFXFloatingTexts()
        {
            Registry.MapEventHooksClient.Register<SkillIncreaseEvent>(OnSkillIncreased);
        }

        private static void OnSkillIncreased(SkillIncreaseEvent e)
        {
            var actor = e.Actor;
            var skill = e.Actor.Skills[e.Skill];
            //FloatingText.Create(actor, $"{skill.SkillDef.Label} increased!", ft => ft.Font = UIManager.FontBold);
            FloatingText.Create(actor.Map, actor.Global, $"{skill.SkillDef.Label} increased!", ft => ft.Font = UIManager.FontBold);
        }
    }
}
