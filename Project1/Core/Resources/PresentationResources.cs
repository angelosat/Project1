using Microsoft.Xna.Framework;
using Project1.Core.Systems.Presentation;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Resources;

internal sealed class PresentationResources : IPresentationWorker
{
    public void Register()
    {
        Registry.MapEventHooksClient.Register<ResourceDeltaAppliedEvent>(OnHealthLost);
    }

    private static void OnHealthLost(ResourceDeltaAppliedEvent e)
    {
        if (e.Def != ResourceDefOf.Health)
            return;
        var dmg = e.Delta;
        var recipient = e.Entity;
        var floating = new FloatingText(recipient, dmg.ToString()) { Font = UIManager.FontBold, TextColorFunc = () => Color.Red };
        floating.Show();
    }
}
