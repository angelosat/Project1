using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Framework.UI;
using System;
using System.Linq;

namespace Project1.Core.UI.Blocks
{
    class BlockOwnerGui : GroupBox, ISelectionBound
    {
        //ComboBoxNewNew<Actor> ComboBox = new ComboBoxNewNew<Actor>(actors, 100, a => a?.Name ?? "-None-", a => PlayerSetOwner(comp, a), () => entity.Map?.World.GetEntity<Actor>(comp.Owner));

        Action unsub;
        public ISelectable CurrentSelection { get; set; }

        public void OnBind(ISelectable selectable)
        {
            if (selectable is not BlockEntity entity ||
               !entity.Comps.TryGetComp<BlockOwnershipComp>(out var comp))
                return;

            this.CurrentSelection = selectable;
            var currentOwner = comp.Owner;
            var actors = entity.Map.Town.Members.Prepend(null);

            var combo = new ComboBoxNewNew<Actor>(actors, 100, a => a?.Name ?? "-None-", a => PlayerSetOwner(comp, a), () => entity.Map?.World.GetEntity<Actor>(comp.Owner));
            this.Controls.Clear();
            this.Controls.Add(combo);

            unsub?.Invoke();
            unsub = entity.Map.Events.ListenTo<BlockEntityRemovedEvent>(e =>
            {
                if (e.Entity == this.CurrentSelection)
                {
                    unsub?.Invoke();
                    this.Window.Hide();
                }
            });
        }

        private static void PlayerSetOwner(BlockOwnershipComp comp, Actor a)
        {
            Ingame.Instance.Events.Post(new PlayerChangedBlockOwnerEvent(comp.Parent, a));
        }
    }
}
