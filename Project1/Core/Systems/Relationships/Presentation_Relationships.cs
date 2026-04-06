using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Networking;
using Project1.Core.Systems.Presentation;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Systems.Relationships;

internal class Presentation_Relationships : IPresentationWorker
{
    public void Register()
    {
        Registry.WorldEventHooksClient.Register<RelationshipDeltaAppliedEvent>(HandleRelationshipDeltaApplied);
    }

    private void HandleRelationshipDeltaApplied(RelationshipDeltaAppliedEvent e)
    {
        var actor = Client.Instance.World.Get<Entity>(e.Owner);
        if (actor.Map is null)
            return;
        //FloatingText.Create(owner.Map, owner.Global, $"{e.Delta:}", ft => ft.Font = UIManager.FontBold);
        var positive = e.Delta > 0;

        FloatingText.Create(
           actor.Map,
           actor.Global,
           //$"{e.Delta:+0;-#}",
           e.Delta > 0 ? "+" : "-",
           ft => {
               ft.Font = UIManager.FontBold;
               ft.TextColorFunc = () => positive ? Color.Lime : Color.Red;
           });
    }
}
