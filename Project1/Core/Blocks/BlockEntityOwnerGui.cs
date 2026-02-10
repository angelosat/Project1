using System;
using System.Linq;
using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.Blocks
{
    class BlockEntityOwnerGui : GroupBox, ISelectionBound
    {
        public ISelectable CurrentSelection { get; set; }

        public void OnBind(ISelectable selectable)
        {
            if (selectable is not TargetArgs target ||
               target.BlockEntityOld is not BlockEntity entity ||
               !entity.Comps.TryGetComp<BlockOwnershipComp>(out var comp))
                throw new Exception();

            var currentOwner = comp.Owner;
            var actors = entity.Map.World.GetEntities(entity.Map.Town.Members).Cast<Actor>().Prepend(null);

            var combo = new ComboBoxNewNew<Actor>(actors, 100, a => a?.Name ?? "-None-", a=> PlayerSetOwner(comp, a), () => entity.Map.World.GetEntity<Actor>(comp.Owner));

            this.Controls.Add(combo);
        }

        private static void PlayerSetOwner(BlockOwnershipComp comp, Actor a)
        {
            Ingame.Instance.Events.Post(new PlayerChangedBlockOwnerEvent(comp.Parent, a));
            // client prediction
            //comp.SetOwner(a);
        }
    }
}
