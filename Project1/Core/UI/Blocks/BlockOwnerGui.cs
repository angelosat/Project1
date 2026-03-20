using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.UI.Blocks
{
    class BlockOwnerGui : SelectionBoundControl
    {
        protected internal override void OnBind(ISelectable selectable)
        {
            if (selectable is not BlockEntity entity ||
               !entity.Comps.TryGetComp<BlockOwnershipComp>(out var comp))
                return;

            var currentOwner = comp.Owner;
            var actors = entity.Map.Town.Members.Prepend(null);

            var combo = new ComboBoxNewNew<Actor>(actors, 100, a => a?.Name ?? "-None-", a => PlayerSetOwner(comp, a), () => entity.Map?.World.GetEntity<Actor>(comp.Owner));
            this.Controls.Clear();
            this.Controls.Add(combo);
        }
        
        protected override void RegisterInvalidations()
        {
            if (this.CurrentSelection is not BlockEntity entity)
                return;

            this.InvalidateOn<BlockEntityRemovedEvent>(
                e => e.Entity == entity);
        }
        private static void PlayerSetOwner(BlockOwnershipComp comp, Actor a)
        {
            Ingame.Instance.Events.Post(new PlayerChangedBlockOwnerEvent(comp.Parent, a));
        }
    }
}
