using Microsoft.Xna.Framework;
using Project1.Core.Systems.Presentation;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Reputation;

sealed class PresentationReputation : IPresentationWorker
{
    public void Register()
    {
        Registry.MapEventHooksClient.Register<ReputationDeltaAppliedEvent>(OnReputationChanged);
    }
    private static void OnReputationChanged(ReputationDeltaAppliedEvent e)
    {
        //var actor = Ingame.Net.World.Get<Actor>(e.ActorId);
        var actor = e.Actor;
        var positive = e.Delta > 0;
        FloatingText.Create(
            actor.Map, 
            actor.Global, 
            $"{e.Delta:+0;-#} Rep", 
            ft => { 
                ft.Font = UIManager.FontBold; 
                ft.TextColorFunc = () => positive ? Color.Lime : Color.Red; });
    }
}
