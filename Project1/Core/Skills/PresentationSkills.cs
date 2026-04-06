using Project1.Core.Systems.Presentation;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Skills
{
    internal sealed class PresentationSkills : IPresentationWorker
    {
        public void Register()
        {
            Registry.WorldEventHooksClient.Register<SkillLevelUpEvent>(OnSkillIncreased);
        }
        private static void OnSkillIncreased(SkillLevelUpEvent e)
        {
            var actor = e.Actor;
            if (!actor.IsSpawned)
                return;
            var skill = e.Actor.Skills[e.Skill.Def];
            FloatingText.Create(actor.Map, actor.Global, $"{skill.SkillDef.LabelReadable} increased!", ft => ft.Font = UIManager.FontBold);
        }
    }
}
