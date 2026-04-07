using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.UI
{
    class UINpcFrameContainer : GroupBox
    {
        List<Actor> TrackedTownMembers = [];
        const int Spacing = 1;//5;
        MapBase Map;
        public UINpcFrameContainer(MapBase map)
        {
            this.Map = map;
            this.HideAction += map.World.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
            this.HideAction += map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
        }

        private void OnEntityDisposed(EntityDisposedEvent e)
        {
            RemoveControl(e.Entity as Actor);
        }

        private void OnEntityDespawned(EntityDespawnedEvent e)
        {
            RemoveControl(e.Entity as Actor);
        }

        private bool RemoveControl(Actor actor)
        {
            if (!this.TrackedTownMembers.Contains(actor))
                return false;
            this.TrackedTownMembers.Remove(actor);
            this.Controls.RemoveAll(c => c.Tag == actor);
            this.AlignLeftToRight(Spacing);
            return true;
        }

        public override void Update()
        {
            if (!Camera.DrawnOnce)
                return;
            //var actors = Net.Client.Instance.Map.Town.GetMembers().Where(a => a != null).ToList(); // WHY WOULD THE RETURNED TOWN MEMBER LIST CONTAIN NULL VALUES???
            var actors = this.Map.Town.GetMembers();
            var toInit = actors.Where(a => !this.TrackedTownMembers.Contains(a));
            foreach (var a in toInit)
                this.AddControlsTopRight(Spacing, new UINpcFrame(a));
            var toRemove = this.TrackedTownMembers.Where(a => !actors.Contains(a));
            if (toRemove.Any())
            {
                foreach (var a in toRemove)
                {
                    this.Controls.RemoveAll(c => c.Tag == a);
                }
                this.AlignLeftToRight(Spacing);
            }
            this.TrackedTownMembers = [.. actors];
            base.Update();
        }
    }
}
